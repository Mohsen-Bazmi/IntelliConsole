using Xunit;
using System.Reactive.Linq;
using FluentAssertions;
using System.Reactive.Subjects;
using System.Reactive.Concurrency;
using System.Text;
using Microsoft.Reactive.Testing;
using System.Collections.Generic;
using System;
using FsCheck.Xunit;
using FsCheck;
using AutoFixture.Xunit2;
using System.Linq;

namespace IntelliConsole.Tests
{
    public class ConsoleMockTests
    {
        // [Fact]
        public void TestName()
        {
            var sut = ConsoleMock.Instance;
            sut.ScheduleTypeLine("something");
            sut.KeyPress.SkipLast(1)
            .Aggregate("", (a, x) => a + char.ToString(x.KeyChar))
            .Wait().Should().Be("something");
        }
        [Fact]
        public void TestName2()
        {
            var ar = new List<string>();
            var subject = new Subject<string>();
            subject.Subscribe(ar.Add);
            subject.OnNext("something");
            subject.OnCompleted();
            // subject.Wait().Should().Be("something");
            ar.Should().Contain("something");
        }
        [Fact]
        public void Does_not_increase_the_cursor_left_by_writing_right_arrow()
        {
            var sut = ConsoleMock.Instance;
            var original = sut.CursorLeft;
            sut.Write(NewConsoleKeyInfo.FromKey(ConsoleKey.RightArrow).KeyChar);
            sut.CursorLeft.Should().Be(original);
        }
        [Theory, AutoData]
        public void Increases_the_cursor_left_by_length_of_the_string_being_written(string something)
        {
            var sut = ConsoleMock.Instance;
            var original = sut.CursorLeft;
            sut.Write(something.ToCharArray());
            sut.CursorLeft.Should().Be(original + something.Length);
        }
        [Fact]
        public void XX()
        {
            var sut = ConsoleMock.Instance;
            var originalLeft = sut.CursorLeft;
            foreach (char c in "dummy")
                sut.Write(c);
            sut.CursorLeft = originalLeft;
            foreach (char c in "dummy")
                sut.Write(c);

            sut.Start();

            sut.DisplayingLine.Should().Be("dummy");
        }
        [Fact]
        public void Overwrites_the_existing_text_on_the_same_cursor_left()
        {
            var sut = ConsoleMock.Instance;
            foreach (char c in "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890")
                sut.Write(c);
            sut.CursorLeft = 0;
            foreach (char c in "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890")
                sut.Write(c);

            sut.Start();

            sut.DisplayingLine.Should().Be("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890");
        }

        [Fact]
        public void Moves_the_cursor_to_next_line_when_the_text_overflows_the_line()
        {
            var sut = ConsoleMock.Instance;
            sut.BufferWidth = 2;
            sut.Write("123".ToCharArray());
            sut.CursorTop.Should().Be(1);
        }
        [Theory]
        [InlineData("ab", " ", 1, "a ")]
        [InlineData("abc", " ", 1, "a c")]
        [InlineData("abcde", " ", 3, "abc e")]
        [InlineData("abcde", " $", 3, "abc $")]
        [InlineData("abcde", " $", 1, "a $de")]
        [InlineData("abcde", "$123", 4, "abcd$123")]
        public void Does_not_shift_the_text_while_inserting_on_the_behalf_of_the_string(
            string old, string strNew, int position, string expected)
        {
            var sut = ConsoleMock.Instance;
            sut.DisplayingLine = old;
            sut.CursorLeft = position;
            sut.Write(strNew.ToCharArray());
            sut.DisplayingLine.Should().Be(expected);
        }
        [Fact]
        public void Slash_b_space_slash_b_removes_the_last_char()
        {
            var sut = ConsoleMock.Instance;
            sut.Write("123");
            sut.Backspace();
            sut.DisplayingLine.Should().Be("12");
        }
        [Fact]
        public void Schedules_multipleLines()
        {
            var sut = ConsoleMock.Instance;
            sut.ScheduleTypeLine("A");
            sut.ScheduleTypeLine("B");

            var keys = sut.KeyPress.Subscribe();

            sut.Start();

            keys.Select(k => k.Key).Should().Contain(ConsoleKey.B);
            // sut.IsMovedToTheNextLine.Should().BeTrue();
            // sut.DisplayingLine.Should().EndWith("line2");
        }
        [Fact]
        public void Schedules_multipleLines2()
        {
            var sut = ConsoleMock.Instance;
            sut.ScheduleTypeLine("A");
            sut.ScheduleTypeLine("B");

            var line = string.Empty;
            var keys = sut.KeyPress.SelectMany(x =>
                        {
                            if (x.Key == ConsoleKey.Enter)
                            {
                                var result = Observable.Return(line);
                                line = string.Empty;
                                return result;
                            }
                            line += char.ToString(x.KeyChar);
                            return Observable.Empty<string>();
                        }).Subscribe();

            sut.Start();

            keys.Last().Should().Be("B");
            // sut.IsMovedToTheNextLine.Should().BeTrue();
            // sut.DisplayingLine.Should().EndWith("line2");
        }

    }
}