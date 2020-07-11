using System;
using System.Linq;

namespace IntelliConsole
{
    class KeyHandler
    {
        readonly IHistoryNarrator historyNarrator;
        readonly ISuggestions suggestions;
        public KeyHandler(IHistoryNarrator historyNarrator, ISuggestions suggestions)
        {
            this.historyNarrator = historyNarrator;
            this.suggestions = suggestions;
        }
        public virtual LineContext Handle(ConsoleKeyInfo keyInfo, LineContext lineCtx)
        => keyInfo.Key switch
        {
            ConsoleKey.UpArrow => OnArrowUp(lineCtx),
            ConsoleKey.DownArrow => OnArrowDown(lineCtx),
            ConsoleKey.LeftArrow => OnLeftArrow(lineCtx),
            ConsoleKey.B when keyInfo.Modifiers == ConsoleModifiers.Control => OnLeftArrow(lineCtx),
            ConsoleKey.RightArrow => OnRightArrow(lineCtx),
            ConsoleKey.End => OnEnd(lineCtx),
            ConsoleKey.Home => OnHome(lineCtx),
            ConsoleKey.A when keyInfo.Modifiers == ConsoleModifiers.Control => OnHome(lineCtx),
            ConsoleKey.Tab => OnTab(lineCtx),
            ConsoleKey.Backspace => OnBackspace(lineCtx),
            ConsoleKey.Delete => OnDelete(lineCtx),
            _ => RecordChar(lineCtx, keyInfo.KeyChar)
        };




        protected virtual LineContext OnHome(LineContext lineCtx)
        => lineCtx.WithCursorLeft(0);

        protected virtual LineContext OnEnd(LineContext lineCtx)
        => lineCtx.WithCursorLeft(lineCtx.DisplayingLine.Length);


        protected virtual LineContext CompleteTheSuggestion(LineContext lineCtx)
        {
            var lineWords = lineCtx.DisplayingLine.Split(' ');
            var matchedSuggestions = suggestions.ThatComplete(lineWords.Last());
            if (matchedSuggestions.Length == 0)
                return lineCtx;

            // Console.WriteLine();
            var previousWords = string.Join(' ', lineWords.SkipLast(1));
            if (lineWords.Length > 1)
                previousWords += ' ';
            var line = previousWords + matchedSuggestions[0];
            return lineCtx.WithLine(line)
                          .WithCursorLeft(line.Length);
        }
        protected virtual LineContext OnTab(LineContext lineCtx)
        => CompleteTheSuggestion(lineCtx);


        protected virtual LineContext OnBackspace(LineContext lineCtx)
        {
            if (string.IsNullOrEmpty(lineCtx.DisplayingLine))
                return lineCtx.WithCursorLeft(lineCtx.CursorLeft);
            if (lineCtx.CursorLeft == 0)
                return lineCtx.WithCursorLeft(lineCtx.CursorLeft);
            var currentLine = lineCtx.DisplayingLine[0..(lineCtx.CursorLeft - 1)]
                            + lineCtx.DisplayingLine[lineCtx.CursorLeft..];
            return lineCtx
            .WithLine(currentLine)
            .WithCursorLeft(lineCtx.CursorLeft - 1);
        }
        protected virtual LineContext OnDelete(LineContext lineCtx)
        {
            if (lineCtx.CursorLeft == lineCtx.DisplayingLine.Length)
                return lineCtx;
            var line = lineCtx.DisplayingLine[0..lineCtx.CursorLeft]
                     + lineCtx.DisplayingLine[(lineCtx.CursorLeft + 1)..];
            return lineCtx.WithLine(line);
        }

        protected virtual LineContext OnLeftArrow(LineContext lineCtx)
        {
            if (lineCtx.CursorLeft == 0)
                return lineCtx;
            // if (lineCtxWriter.CursorLeft == 0)
            // {
            //     lineCtxWriter.CursorTop--;
            //     lineCtxWriter.CursorLeft = lineCtxWriter.BufferWidth;
            //     return new KeyPresslineCtx { currentLine = lineCtx.currentLine, localCursorLeft =localCursorLeft};
            // }
            // lineCtxWriter.CursorLeft--;

            // localCursorLeft--;
            return lineCtx.WithCursorLeft(lineCtx.CursorLeft - 1);
        }

        protected virtual LineContext OnRightArrow(LineContext lineCtx)
        {
            if (lineCtx.CursorLeft + 1 > lineCtx.DisplayingLine.Length)
                return lineCtx;
            // if (lineCtxWriter.CursorLeft == lineCtxWriter.BufferWidth)
            // {
            //     lineCtxWriter.CursorLeft = 0;
            //     lineCtxWriter.CursorTop++;
            //     return new KeyPressViewModelineCtx.localCursorLeft + str.Length

            // localCursorLeft++;
            return lineCtx.WithCursorLeft(lineCtx.CursorLeft + 1);
        }


        protected virtual bool IsCharAllowed(char c)
        => c != '\0';

        protected virtual LineContext RecordChar(LineContext lineCtx, char c)
        {
            if (!IsCharAllowed(c))
                return lineCtx;
            var str = char.ToString(c);
            if (string.IsNullOrEmpty(str))
                return lineCtx;
            string localCurrentLine;
            try
            {
                localCurrentLine = lineCtx.DisplayingLine.Insert(lineCtx.CursorLeft, char.ToString(c));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw new ArgumentOutOfRangeException($@"
cursorLeft:{lineCtx.CursorLeft},line:{lineCtx.DisplayingLine},char:{c}", ex);
            }
            return lineCtx.WithLine(localCurrentLine)
                          .WithCursorLeft(lineCtx.CursorLeft + str.Length);
        }


        protected LineContext PreviousLine(LineContext lineCtx)
        {
            if (!historyNarrator.MovePrevious())
                return lineCtx;
            return lineCtx.WithLine(historyNarrator.Current)
                          .WithCursorLeft(historyNarrator.Current.Length);
        }
        protected virtual LineContext OnArrowUp(LineContext lineCtx)
        {
            return PreviousLine(lineCtx);
        }

        protected LineContext NextLine(LineContext lineCtx)
        {
            if (!historyNarrator.MoveNext())
                return lineCtx;
            return lineCtx.WithLine(historyNarrator.Current)
                          .WithCursorLeft(historyNarrator.Current.Length);
        }
        protected virtual LineContext OnArrowDown(LineContext lineCtx)
        {
            return NextLine(lineCtx);
        }
    }
}
