using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using Microsoft.Reactive.Testing;
namespace IntelliConsole.Tests
{
    sealed class ConsoleMock : IConsoleObserver
                     , IConsoleWriter
    {
        public static ConsoleMock Instance
        => new ConsoleMock();
        ConsoleMock()
        {
            subject = new ReplaySubject<ConsoleKeyInfo>(ImmediateScheduler.Instance);
            Scheduler = new TestScheduler();
        }

        List<Recorded<Notification<ConsoleKeyInfo>>> notifications = new List<Recorded<Notification<ConsoleKeyInfo>>>();
        public void ScheduleKeyInfo(ConsoleKeyInfo keyInfo)
        => notifications.Add(new Recorded<Notification<ConsoleKeyInfo>>(0, Notification.CreateOnNext(keyInfo)));

        TestScheduler Scheduler { get; }
        readonly ISubject<ConsoleKeyInfo> subject;

        IObservable<ConsoleKeyInfo> keyPress;
        public IObservable<ConsoleKeyInfo> KeyPress
        {
            get => keyPress ?? Scheduler.CreateColdObservable(notifications.ToArray());
            set => keyPress = value;
        }
        // => subject;

        public int LeftStartPosition => 0;
        public int CursorLeft { get; set; }

        public void ScheduleTypeLine(string text)
        {

            ScheduleType(text);
            ScheduleEnter();
        }
        public void TypeLine(ConsoleKey key)
        {
            ScheduleKey(key);
            ScheduleEnter();
        }
        public void ScheduleEnter()
        => ScheduleKey(ConsoleKey.Enter);

        public void ScheduleType(string text)
        {
            if (text == null)
                return;
            foreach (char c in text)
                ScheduleKey((ConsoleKey)c);
        }


        public void ScheduleKey(ConsoleKey key)
        => ScheduleKeyInfo(NewConsoleKeyInfo.FromKey(key));

        public void Start()
        => Scheduler.Start();



        public void WriteLine()
        => IsMovedToTheNextLine = true;

        public void WriteLine(string line)
        => PreviousLine = line;

        public bool IsMovedToTheNextLine { get; private set; } = false;
        public void Write(string strNew)
        {
            // if (strNew == "\b \b")
            //     DisplayingLine = new string(DisplayingLine.SkipLast(1).ToArray());
            // else
            strNew.ToCharArray().ToList().ForEach(c => Write(c));
        }
        public void Backspace()
        => DisplayingLine = DisplayingLine[0..^1];
        public void Write(params char[] c)
        {
            var strNew = new string(c);
            var landingCursorLeft = Math.Min(CursorLeft + strNew.Length, DisplayingLine.Length);
            DisplayingLine = DisplayingLine.Substring(0, CursorLeft)
            + strNew
            + DisplayingLine.Substring(landingCursorLeft);

            CursorTop = (DisplayingLine.Length - 1) / BufferWidth;
            CursorLeft += c.Length - c.Count(x => x == NewConsoleKeyInfo.FromKey(ConsoleKey.RightArrow).KeyChar);
        }
        public void SetCursorPosition(int left, int top)
        {
            CursorLeft = left;
            CursorTop = top;
        }
        public string DisplayingLine { get; set; } = "";
        public string PreviousLine { get; set; } = "";

        public int BufferWidth { get; set; } = 150;
        public int CursorTop { get; set; }
        public int BufferHeight { get; } = 34;

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


    }
}
