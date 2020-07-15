using System;
using System.Reactive.Linq;

namespace IntelliConsole
{
    public class ConsoleProxy
               : IConsoleObserver
               , IConsoleWriter
    {
        public IObservable<ConsoleKeyInfo> KeyPress
        => Observable.Defer(() =>
            Observable.Start(() =>
                Console.ReadKey(true)))
          .Repeat();

        public int CursorLeft
        {
            get => Console.CursorLeft;
            set => Console.CursorLeft = value;
        }

        public void SetCursorPosition(int left, int top)
        => Console.SetCursorPosition(left, top);
        public void Write(string str)
        => Console.Write(str);
        public void Write(params char[] c)
        => Console.Write(c);

        public void WriteLine()
        => Console.WriteLine();

        public void WriteLine(string line)
        => Console.WriteLine(line);

        public int BufferWidth => Console.BufferWidth;
        public int CursorTop
        {
            get => Console.CursorTop;
            set => Console.CursorTop = value;
        }
        public int PromptTop { get; set; }
        public int PromptLength { get; set; }
        public int BufferHeight
        => Console.BufferHeight;

        public ConsoleColor BackgroundColor
        {
            get => Console.BackgroundColor;
            set => Console.BackgroundColor = value;
        }
        public ConsoleColor ForegroundColor
        {
            get => Console.ForegroundColor;
            set => Console.ForegroundColor = value;
        }
        public void Backspace()
        {
            void RemoveTheLastCharFromCurrentLine()
            {
                Write("\b \b");
                // CursorLeft--;
                // Write(' ');
                // if (CursorLeft > 0)
                //     CursorLeft--;
            }
            void RemoveTheLastCharFromPreviousLine()
            {
                CursorTop--;
                CursorLeft = BufferWidth;
                Write(' ');
                CursorTop--;
                CursorLeft = BufferWidth - 1;
            }

            //Write("\b \b");
            if (CursorLeft > 0)
                RemoveTheLastCharFromCurrentLine();
            else
                RemoveTheLastCharFromPreviousLine();
        }


    }
}