using System;

namespace IntelliConsole
{
    public interface IConsoleObserver
    {
        IObservable<ConsoleKeyInfo> KeyPress { get; }
    }
    public interface IConsoleWriter
    {
        int BufferWidth { get; }
        void WriteLine();
        void WriteLine(string line);
        void Write(string str);
        void Write(params char[] c);
        int CursorLeft { get; set; }
        int CursorTop { get; set; }
        int BufferHeight { get; }
        ConsoleColor BackgroundColor { get; set; }
        ConsoleColor ForegroundColor { get; set; }

        void SetCursorPosition(int left, int top);
        void Backspace();
    }
}