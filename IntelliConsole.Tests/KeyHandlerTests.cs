// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using AutoFixture.Xunit2;
// using FluentAssertions;
// using FsCheck;
// using FsCheck.Xunit;
// using Xunit;

// namespace IntelliConsole.Tests
// {
//     public class KeyHandlerTests
//     {
//         ConsoleMock console = new ConsoleMock();
//         public KeyHandlerTests()
//         {
//             sut = new KeyHandler(console);
//         }
//         readonly KeyHandler sut;

//         [Theory]
//         [AutoData]
//         [InlineData("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890")]
//         public void Returns_the_words_as_string_by_passing_enter_key(string expected)
//         {
//             foreach (char c in expected)
//                 sut.Handle(NewConsoleKeyInfo.FromChar(c));
//             sut.Handle(NewConsoleKeyInfo.FromKey(ConsoleKey.Enter))

//             .Should().Be(new string(expected));
//         }

//         [Theory]
//         [AutoData]
//         [InlineData("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890")]
//         public void Appends_characters_to_console(string expected)
//         {
//             foreach (char c in expected)
//                 sut.Handle(NewConsoleKeyInfo.FromChar(c));
//             console.CurrentLine.Should().Be(expected ?? "");
//         }

//         [Fact]
//         public void Left_arrow_moves_the_cursor_to_previous_position()
//         {
//             var original = console.CursorLeft;
//             sut.Handle(NewConsoleKeyInfo.FromKey(ConsoleKey.LeftArrow));
//             console.CursorLeft.Should().Be(original - 1);
//         }

//         [Fact]
//         public void Right_arrow_moves_the_cursor_to_previous_position()
//         {
//             var original = console.CursorLeft;
//             sut.Handle(NewConsoleKeyInfo.FromKey(ConsoleKey.RightArrow));
//             console.CursorLeft.Should().Be(original + 1);
//         }

//         [Fact]
//         public void Inserts_the_text_to_current_position()
//         {
//             foreach (char c in "ab")
//                 sut.Handle(NewConsoleKeyInfo.FromChar(c));
//             sut.Handle(NewConsoleKeyInfo.FromKey(ConsoleKey.LeftArrow));
//             sut.Handle(NewConsoleKeyInfo.FromKey(ConsoleKey.Spacebar));
//             // sut.Handle(NewConsoleKeyInfo.FromKey(ConsoleKey.Enter))
//             console.CurrentLine.Should().Be("a b");
//         }

//         // [Fact]
//         // public void XXXXXXXXX()
//         // {
//         //     var state = new List<char>();
//         //     state.Insert(consoleWriter.CursorLeft,
//         // }
//         //Delete (+enter)
//         //ctrl+left/right (+enter)

//         // [Fact]
//         // public void xxxx()
//         // {
//         //     Func<string, bool> tst = expected =>
//         //     {
//         //         foreach (char c in expected)
//         //             x += char.ToString(c);
//         //         return x == expected;
//         //     };
//         //     Prop.ForAll(tst).QuickCheckThrowOnFailure();
//         // }

//         // string x;
//     }
// }