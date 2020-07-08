using Xunit;
using FluentAssertions;
using FsCheck.Xunit;
using System.Reactive.Linq;
using System;
using System.Collections.Generic;
using AutoFixture.Xunit2;
using System.Linq;
using Microsoft.Reactive.Testing;

namespace IntelliConsole.Tests
{
    public class ConsoleReaderTests : ReactiveTest
    {
        readonly ConsoleMock console = ConsoleMock.Instance;
        readonly ConsoleReader sut;

        ConsoleReader CreateSut(params string[] suggestions)
        => new ConsoleReader(console, console
                           , SyntaxHighlighting.Highlight
                           , History.Ancient
                           , Suggestions.From(suggestions));
        public ConsoleReaderTests()
        {
            sut = CreateSut();
        }


        [Theory]
        [InlineData(" ")]
        [InlineData("a text")]
        [InlineData(" Hello World ")]
        [InlineData("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890")]
        // [Property(Arbitrary = new[] { typeof(GenerateNonNullOrEnterStrings) })]
        public void Returns_the_text_typed_in_the_console(string helloWorld)
        {
            console.ScheduleTypeLine(helloWorld);

            var result = sut.ObserveLines().Subscribe();
            console.Start();

            result.Should().Contain(helloWorld);
        }
        // //Should have moved to the next line.
        [Theory]
        // [AutoData]
        [InlineData("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890")]
        // [InlineData("a")]
        public void Appends_characters_to_console(string helloWorld)
        {
            console.ScheduleType(helloWorld);

            sut.ObserveLines().Subscribe();
            console.Start();

            console.DisplayingLine.Should().Be(helloWorld);
        }

        [Fact]
        public void Does_not_emit_before_pressing_the_enter()
        {
            console.ScheduleKey(ConsoleKey.A);
            console.ScheduleKey(ConsoleKey.LeftArrow);

            var result = sut.ObserveLines().Subscribe();
            console.Start();

            result.Should().BeEmpty();
        }

        [Fact]
        public void Left_arrow_moves_the_cursor_to_previous_position()
        {
            console.ScheduleType("dummy");
            console.ScheduleKey(ConsoleKey.LeftArrow);

            sut.ObserveLines().Subscribe();
            console.Start();

            console.CursorLeft.Should().Be("dummy".Length - 1);
        }
        [Fact]
        public void Control_b_moves_the_cursor_to_previous_position()
        {
            console.ScheduleType("dummy");
            console.ScheduleKeyInfo(new ConsoleKeyInfo('b', ConsoleKey.B, false, false, true));

            sut.ObserveLines().Subscribe();
            console.Start();

            console.CursorLeft.Should().Be("dummy".Length - 1);
        }

        [Fact]
        public void Left_arrow_does_not_move_the_cursor_more_than_its_original_position()
        {
            var original = console.CursorLeft;
            console.ScheduleKey(ConsoleKey.LeftArrow);

            sut.ObserveLines().Subscribe();
            console.Start();

            console.CursorLeft.Should().Be(original);
        }
        [Fact]
        public void Left_moves_the_cursor_to_previous_line_in_multiline_scenario()
        {
            console.BufferWidth = 2;
            console.ScheduleType("123");
            console.ScheduleKey(ConsoleKey.LeftArrow);
            console.ScheduleKey(ConsoleKey.LeftArrow);

            sut.ObserveLines().Subscribe();
            console.Start();

            console.CursorTop.Should().Be(0);
            console.CursorLeft.Should().Be(1);
        }

        [Fact]
        public void Right_arrow_moves_the_cursor_to_previous_position()
        {
            console.ScheduleType("s");
            console.ScheduleKey(ConsoleKey.RightArrow);

            sut.ObserveLines().Subscribe();
            var original = console.CursorLeft;
            console.Start();

            console.CursorLeft.Should().Be(original + 1);
        }
        [Theory, AutoData]
        public void Typing_moves_the_cursor_to_the_next_line_when_text_fills_the_line(string fullLine)
        {
            console.BufferWidth = fullLine.Length;
            console.ScheduleType(fullLine);

            var originalTop = console.CursorTop;
            sut.ObserveLines().Subscribe();
            console.Start();

            console.CursorLeft.Should().Be(0);
            console.CursorTop.Should().Be(originalTop + 1);
        }
        [Fact]
        public void Right_arrow_moves_the_cursor_to_the_next_line_when_reaches_the_end_of_the_line()
        {
            console.BufferWidth = 3;
            console.ScheduleType("123");
            console.ScheduleKey(ConsoleKey.RightArrow);

            sut.ObserveLines().Subscribe();
            var originalTop = console.CursorTop;
            console.Start();

            console.CursorLeft.Should().Be(1);
            console.CursorTop.Should().Be(originalTop + 1);
        }


        [Fact]
        public void Inserts_the_text_to_current_position_and_shifts_the_rest()
        {
            console.ScheduleType("ab");
            console.ScheduleKey(ConsoleKey.LeftArrow);
            console.ScheduleKey(ConsoleKey.Spacebar);

            sut.ObserveLines().Subscribe();
            console.Start();

            console.DisplayingLine.Should().Be("a b");
        }


        [Theory, AutoData]
        public void Home_moves_the_cursor_to_the_line_start(string text)
        {
            console.ScheduleType(text);
            console.ScheduleKey(ConsoleKey.Home);

            sut.ObserveLines().Subscribe();
            console.Start();

            console.CursorLeft.Should().Be(console.LeftStartPosition);
        }
        [Theory, AutoData]
        public void Backspace_does_not_work_at_home(string text)
        {
            console.ScheduleType(text);
            console.ScheduleKey(ConsoleKey.Home);
            console.ScheduleKey(ConsoleKey.Backspace);

            sut.ObserveLines().Subscribe();
            console.Start();

            console.DisplayingLine.Should().Be(text);
            console.CursorLeft.Should().Be(console.LeftStartPosition);
        }
        [Theory, AutoData]
        public void Delete_does_not_work_when_the_cursor_is_at_the_end_of_line(string text)
        {
            console.ScheduleType(text);
            console.ScheduleKey(ConsoleKey.End);
            console.ScheduleKey(ConsoleKey.Delete);

            sut.ObserveLines().Subscribe();
            console.Start();

            console.DisplayingLine.Should().Be(text);
            console.CursorLeft.Should().Be(text.Length);
        }


        [Fact]
        public void Delete_removes_the_next_char()
        {
            console.ScheduleType("dummy");
            console.ScheduleKey(ConsoleKey.LeftArrow);
            console.ScheduleKey(ConsoleKey.Delete);

            sut.ObserveLines().Subscribe();
            console.Start();

            console.DisplayingLine.Should().Be("dumm");
            console.CursorLeft.Should().Be("dumm".Length);
        }
        [Fact]
        public void Delete_does_not_change_an_empty_line()
        {
            console.ScheduleKey(ConsoleKey.Delete);

            sut.ObserveLines().Subscribe();
            console.Start();

            console.DisplayingLine.Should().Be("");
        }
        [Theory, AutoData]
        public void End_moves_the_cursor_to_the_line_end(string text)
        {
            console.ScheduleType(text);
            console.ScheduleKey(ConsoleKey.Home);
            console.ScheduleKey(ConsoleKey.End);

            sut.ObserveLines().Subscribe();
            console.Start();

            console.CursorLeft.Should().Be(text.Length);
        }
        [Theory, AutoData]
        public void Ctrl_a_moves_the_cursor_to_the_line_start(string text)
        {
            console.ScheduleType(text);
            console.ScheduleKeyInfo(new ConsoleKeyInfo('a', ConsoleKey.A, false, false, true));

            sut.ObserveLines().Subscribe();
            console.Start();

            console.CursorLeft.Should().Be(console.LeftStartPosition);
        }


        [Fact]
        public void Backspace_removes_the_last_character()
        {
            console.ScheduleType("dummy");
            console.ScheduleKey(ConsoleKey.Backspace);

            sut.ObserveLines().Subscribe();
            console.Start();

            console.DisplayingLine.Should().Be("dumm");
        }

        [Fact]
        public void Backspace_removes_the_character_before_cursor()
        {
            console.ScheduleType("dummy");
            console.ScheduleKey(ConsoleKey.LeftArrow);
            console.ScheduleKey(ConsoleKey.Backspace);
            console.ScheduleKey(ConsoleKey.Backspace);

            sut.ObserveLines().Subscribe();
            console.Start();

            console.DisplayingLine.Should().Be("duy");
        }

        [Fact]
        public void Backspace_does_not_do_anything_when_displaying_nothing()
        {
            console.ScheduleKey(ConsoleKey.Backspace);

            sut.ObserveLines().Subscribe();
            console.Start();

            console.DisplayingLine.Should().BeEmpty();
            console.CursorLeft.Should().Be(0);
        }

        [Fact]
        public void Up_shows_the_previous_line()
        {
            console.ScheduleTypeLine("line1");
            console.ScheduleTypeLine("line2");
            console.ScheduleType("line3");

            console.ScheduleKey(ConsoleKey.UpArrow);

            sut.ObserveLines().Subscribe();
            console.Start();
            console.IsMovedToTheNextLine.Should().BeTrue();
            console.DisplayingLine.Should().EndWith("line2");
        }
        [Fact]
        public void Up_does_not_show_previous_empty_lines()
        {
            console.ScheduleTypeLine("line1");
            console.ScheduleTypeLine(string.Empty);
            console.ScheduleTypeLine("line3");

            console.ScheduleKey(ConsoleKey.UpArrow);
            console.ScheduleKey(ConsoleKey.UpArrow);

            sut.ObserveLines().Subscribe();
            console.Start();
            console.DisplayingLine.Should().EndWith("line1");
        }
        [Fact]
        public void Up_does_not_do_anything_at_first_line()
        {
            console.ScheduleType("dummy");
            console.ScheduleKey(ConsoleKey.UpArrow);

            sut.ObserveLines().Subscribe();
            console.Start();
            console.DisplayingLine.Should().Be("dummy");
        }

        [Fact]
        public void Down_shows_the_next_line()
        {
            console.ScheduleTypeLine("line1");
            console.ScheduleTypeLine("line2");
            console.ScheduleType("line3");

            console.ScheduleKey(ConsoleKey.UpArrow);
            console.ScheduleKey(ConsoleKey.UpArrow);
            console.ScheduleKey(ConsoleKey.DownArrow);

            sut.ObserveLines().Subscribe();
            console.Start();
            console.IsMovedToTheNextLine.Should().BeTrue();
            console.DisplayingLine.Should().EndWith("line2");
        }
        [Fact]
        public void Down_does_nothing_when_no_more_items()
        {
            console.ScheduleKey(ConsoleKey.DownArrow);

            sut.ObserveLines().Subscribe();
            console.Start();
            console.DisplayingLine.Should().Be("");
        }
        [Fact]
        public void Up_does_nothing_initially()
        {
            console.ScheduleKey(ConsoleKey.UpArrow);

            sut.ObserveLines().Subscribe();
            console.Start();
            console.DisplayingLine.Should().BeEmpty();
        }

        [Fact]
        public void Tab_suggests_the_first_match()
        {
            var sut = CreateSut("Hello", "Hi");

            console.ScheduleType("H");
            console.ScheduleKey(ConsoleKey.Tab);

            sut.ObserveLines().Subscribe();
            console.Start();

            console.PreviousLine.Should().Be("Hello\tHi");
            console.DisplayingLine.Should().Be("H");
        }

        // [Fact]
        // public void Tab_suggests_the_next_match()
        // {
        //     var sut = CreateSut("Hello", "Hi");

        //     console.ScheduleType("H");
        //     console.ScheduleKey(ConsoleKey.Tab);
        //     console.ScheduleKey(ConsoleKey.Tab);

        //     sut.ObserveLines().Subscribe();
        //     console.Start();

        //     console.PreviousLine.Should().Be("Hello\tHi");
        //     console.IsMovedToTheNextLine.Should().BeTrue();
        //     console.DisplayingLine.Should().Be("H");
        // }
        [Fact]
        public void Tab_types_the_suggestion_when_matches_a_single_suggestion()
        {
            var sut = CreateSut("Hello", "Hi");

            console.ScheduleType("bye He");
            console.ScheduleKey(ConsoleKey.Tab);
            console.ScheduleKey(ConsoleKey.Tab);

            sut.ObserveLines().Subscribe();
            console.Start();

            console.CursorLeft.Should().Be("bye Hello".Length);
            console.DisplayingLine.Should().Be("bye Hello");
        }
        [Fact]
        public void Tab_types_the_suggestion_when_a_single_word_line_matches_a_single_suggestion()
        {
            var sut = CreateSut("Hello", "Hi");

            console.ScheduleType("He");
            console.ScheduleKey(ConsoleKey.Tab);
            console.ScheduleKey(ConsoleKey.Tab);

            sut.ObserveLines().Subscribe();
            console.Start();

            console.DisplayingLine.Should().Be("Hello");
            console.CursorLeft.Should().Be("Hello".Length);
        }
        [Fact]
        public void Tab_types_nothing_when_no_suggestions_match()
        {
            var sut = CreateSut("Hello", "Hi");

            console.ScheduleType("Bye");
            console.ScheduleKey(ConsoleKey.Tab);
            console.ScheduleKey(ConsoleKey.Tab);

            sut.ObserveLines().Subscribe();
            console.Start();

            console.PreviousLine.Should().BeEmpty();
            console.DisplayingLine.Should().Be("Bye");
        }
        // [Fact]
        // public void aaaa()
        // {
        //     List<char> state = new List<char>();
        //     console.Type("RA");
        //     console.KeyPress
        //    .TakeUntil(info => info.Key == ConsoleKey.Enter)
        //    .Select(keyInfo =>
        //    {
        //     //    state.Add(keyInfo.KeyChar);
        //     //    var output = string.Join("", state);
        //     //    return output;
        //        if (keyInfo.Key == ConsoleKey.Enter)
        //        {
        //            Console.WriteLine();
        //            var output = string.Join("", state);
        //            state.Clear();
        //            return output;
        //        }
        //        state.Add(keyInfo.KeyChar);
        //        return "";
        //    })
        //    .LastOrDefaultAsync()
        //    .Wait().Trim()
        //     .Should().Be("RA");
        // }
    }
}
