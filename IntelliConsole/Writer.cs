using System;
using System.Linq;

namespace IntelliConsole
{
    class SuggestionPrinter
    {
        public SuggestionPrinter(IConsoleWriter consoleWriter
                     , IColourConsole colourConsole
                     , ISuggestions suggestions)
        {
            this.consoleWriter = consoleWriter;
            this.colourConsole = colourConsole;
            this.suggestions = suggestions;
        }

        readonly IConsoleWriter consoleWriter;
        readonly IColourConsole colourConsole;
        readonly ISuggestions suggestions;
        int printedSuggestionsCount = 0;
        public void ClearPrintedSuggestions()
        {
            var originalTop = consoleWriter.CursorTop;
            var originalLeft = consoleWriter.CursorLeft;
            for (; printedSuggestionsCount > 0; printedSuggestionsCount--)
            {
                consoleWriter.CursorTop = originalTop + printedSuggestionsCount;
                consoleWriter.CursorLeft = consoleWriter.BufferWidth;
                for (var j = consoleWriter.BufferWidth; j > 0; j--)
                    consoleWriter.Write("\b \b");
            }
            consoleWriter.CursorTop = originalTop;
            consoleWriter.CursorLeft = originalLeft;
        }

        public bool CanHandle(ConsoleKeyInfo keyInfo)
        => new[]{ ConsoleKey.Tab, ConsoleKey.Escape,ConsoleKey.UpArrow,ConsoleKey.DownArrow
            }.Contains(keyInfo.Key);


        string previouslyCompletedWord = "";
        string previouslySuggestedWord = "";
        int previousSuggestionIndex;

        // protected virtual LineContext SelectSuggestion(LineContext lineCtx,int index)
        // {

        // }
        protected virtual LineContext SelectNextSuggestion(LineContext lineCtx)
        {
            var lineWords = lineCtx.DisplayingLine.Split(' ');
            var matchedSuggestions = suggestions.ThatComplete(lineWords.Last());
            if (previousSuggestionIndex > 0
            && previousSuggestionIndex == matchedSuggestions.Length - 1)
            {
                previousSuggestionIndex = 0;
            }
            else
            {
                previousSuggestionIndex++;
            }
            SuggestBasedOn(lineCtx.DisplayingLine);
            return lineCtx;
        }
        protected virtual LineContext SelectPreviousSuggestion(LineContext lineCtx)
        {
            var lineWords = lineCtx.DisplayingLine.Split(' ');
            var matchedSuggestions = suggestions.ThatComplete(lineWords.Last());
            if (previousSuggestionIndex == 0)
            {
                previousSuggestionIndex = matchedSuggestions.Length - 1;
            }
            else
            {
                previousSuggestionIndex--;
            }
            SuggestBasedOn(lineCtx.DisplayingLine);
            return lineCtx;
        }
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

            // if (previousSuggestionIndex > 0
            // && (previouslyCompletedWord != lineWords.Last()
            // || previousSuggestionIndex == matchedSuggestions.Length - 1))
            // {
            //     previousSuggestionIndex = 0;
            // }
            // else
            // {
            //     previousSuggestionIndex++;
            // }
            var line = previousWords + matchedSuggestions[previousSuggestionIndex];

            previouslyCompletedWord = lineWords.Last();

            previouslySuggestedWord = matchedSuggestions[previousSuggestionIndex];

            return lineCtx.WithLine(line)
                          .WithCursorLeft(line.Length)
                          ;
        }

        internal LineContext Handle(ConsoleKeyInfo keyInfo, LineContext context)
        {
            if (keyInfo.Key == ConsoleKey.Tab)
            {
                return CompleteTheSuggestion(context);
            }
            else if (keyInfo.Key == ConsoleKey.Escape)
            {
                ClearPrintedSuggestions();
            }
            else if (keyInfo.Key == ConsoleKey.UpArrow)
            {
                SelectPreviousSuggestion(context);
            }
            else if (keyInfo.Key == ConsoleKey.DownArrow)
            {
                SelectNextSuggestion(context);
            }
            return context;
        }
        void PrintSuggestionItem(int left, string suggestion
                        , int maxSuggestionsLength, int numberOfMatchedChars)
        {
            consoleWriter.CursorLeft = left;
            // consoleWriter.WriteLine(suggestion);
            consoleWriter.Write('|');
            var originalForeground = consoleWriter.ForegroundColor;

            consoleWriter.ForegroundColor = ConsoleColor.Cyan;
            consoleWriter.Write(suggestion[..numberOfMatchedChars]);

            consoleWriter.Write(suggestion[numberOfMatchedChars]);
            consoleWriter.ForegroundColor = originalForeground;

            consoleWriter.Write(suggestion[(numberOfMatchedChars + 1)..]);
            // for (var i = 0; i < numberOfMatchedChars; i++)
            // {
            //     consoleWriter.Write(suggestion[i]);
            // }
            // for (var i = numberOfMatchedChars; i < suggestion.Length; i++)
            // {
            //     consoleWriter.Write(suggestion[i]);
            // }
            var spaces = "";
            for (var i = maxSuggestionsLength - suggestion.Length; i > 0; i--)
                spaces += ' ';
            // consoleWriter.Write(' ');
            consoleWriter.Write($"{spaces}|");

            consoleWriter.WriteLine();
        }
        public virtual void SuggestBasedOn(string currentLine)
        {

            var theLastWord = currentLine.Split(' ').Last();


            var matchedSuggestions = suggestions.ThatComplete(theLastWord);
            if (matchedSuggestions.Contains(previouslySuggestedWord))
            {
                previousSuggestionIndex = matchedSuggestions.ToList().IndexOf(previouslySuggestedWord);
            }
            else
            {
                previouslyCompletedWord = "";
                previousSuggestionIndex = 0;
            }
            var theLastWordOverflowsConsoleWidth = consoleWriter.CursorLeft - theLastWord.Length < 0;
            var anySuggestionsOverflowConsoleWidth
                = matchedSuggestions.Any(s => s.Length + consoleWriter.CursorLeft
                                            > consoleWriter.BufferWidth);
            if (matchedSuggestions.Length == 0
            || theLastWordOverflowsConsoleWidth
            || anySuggestionsOverflowConsoleWidth
            )
                return;

            var originalTop = consoleWriter.CursorTop;
            var originalLeft = consoleWriter.CursorLeft;
            var left = originalLeft - theLastWord.Length - 1;
            var maxSuggestionsLength = matchedSuggestions.Max(x => x.Length);
            var numberOfMatchedChars = theLastWord.Length - 1;
            consoleWriter.WriteLine();
            var originalBackground = consoleWriter.BackgroundColor;
            for (int i = 0; i < previousSuggestionIndex; i++)
            {
                PrintSuggestionItem(left, matchedSuggestions[i]
                                   , maxSuggestionsLength, numberOfMatchedChars);
            }
            consoleWriter.BackgroundColor = ConsoleColor.DarkMagenta;
            PrintSuggestionItem(left, matchedSuggestions[previousSuggestionIndex]
                               , maxSuggestionsLength, numberOfMatchedChars);

            consoleWriter.BackgroundColor = originalBackground;
            for (int i = previousSuggestionIndex + 1; i < matchedSuggestions.Length; i++)
            {
                PrintSuggestionItem(left, matchedSuggestions[i]
                                   , maxSuggestionsLength, numberOfMatchedChars);
            }
            // foreach (var suggestion in matchedSuggestions)
            // {
            //     PrintSuggestionItem(left, suggestion, maxSuggestionsLength, numberOfMatchedChars);
            //     // consoleWriter.CursorLeft = left;
            //     // consoleWriter.Write('|');
            //     // consoleWriter.Write(suggestion);
            //     // for (var i =  - suggestion.Length; i > 0; i--)
            //     //     consoleWriter.Write(' ');
            //     // consoleWriter.Write('|');
            //     // consoleWriter.WriteLine();
            // }
            // // consoleWriter.CursorLeft = left;
            // // consoleWriter.Write('|');
            // // for (var i = matchedSuggestions.Max(x => x.Length); i > 0; i--)
            // //     consoleWriter.Write('_');
            // // consoleWriter.Write('|');
            // // consoleWriter.WriteLine();

            printedSuggestionsCount = matchedSuggestions.Length + 1;
            var contentHeight = originalTop + matchedSuggestions.Length + 1;
            var windowCapacity = consoleWriter.BufferHeight - 1;
            var overflowenSuggestionCount = contentHeight - windowCapacity;

            if (overflowenSuggestionCount > 0)
            {
                consoleWriter.CursorTop = originalTop - overflowenSuggestionCount;
                consoleWriter.PromptTop -= overflowenSuggestionCount;
            }
            else
            {
                consoleWriter.CursorTop = originalTop;
            }
            consoleWriter.CursorLeft = originalLeft;
        }
    }
    class Writer
    {
        public Writer(IConsoleWriter consoleWriter
                     , IColourConsole colourConsole
                     , ISuggestions suggestions)
        {
            this.consoleWriter = consoleWriter;
            this.colourConsole = colourConsole;
            this.suggestions = suggestions;
        }

        readonly IConsoleWriter consoleWriter;
        readonly IColourConsole colourConsole;
        readonly ISuggestions suggestions;




        public void MoveToNextLine()
        {
            consoleWriter.WriteLine();
            isFirstCharOfCurrentLine = true;
        }
        int previousLen = 0;


        bool isFirstCharOfCurrentLine = true;


        public void UpdateTheCurrentLine(string line, int localCursorLeft)
        {
            if (isFirstCharOfCurrentLine)
            {
                isFirstCharOfCurrentLine = false;
                previousLen = 0;
                consoleWriter.PromptLength = consoleWriter.CursorLeft;
                consoleWriter.PromptTop = consoleWriter.CursorTop;
            }
            var totalLen = consoleWriter.PromptLength + line.Length;

            var cursorIsHidden = totalLen == previousLen;//To balance between speed and UX
            if (cursorIsHidden)
                System.Console.CursorVisible = true;

            ClearExtraCharacters(totalLen);
            MoveCursorNextToThePrompt();
            colourConsole.Write(line);
            MoveCursorToLocalPosition(localCursorLeft);

            if (cursorIsHidden)
                System.Console.CursorVisible = false;
            UpdatePromptTop(totalLen);
            previousLen = totalLen;
        }
        void MoveCursorNextToThePrompt()
        => consoleWriter.SetCursorPosition(consoleWriter.PromptLength, consoleWriter.PromptTop);

        void UpdatePromptTop(int len)
        {
            var lineHeight = len / consoleWriter.BufferWidth;
            var lineOverflowsWidth = consoleWriter.PromptTop + lineHeight >= consoleWriter.BufferHeight;

            if (lineOverflowsWidth && len >= previousLen)
            {
                consoleWriter.PromptTop = consoleWriter.BufferHeight - 1
                                - (len - 1) / consoleWriter.BufferWidth;

            }
        }

        void MoveCursorToLocalPosition(int localCursorLeft)
        {
            var totalCursorLeft = consoleWriter.PromptLength + localCursorLeft;
            consoleWriter.CursorLeft = totalCursorLeft % consoleWriter.BufferWidth;
            consoleWriter.CursorTop = consoleWriter.PromptTop + totalCursorLeft / consoleWriter.BufferWidth;
        }

        void ClearExtraCharacters(int currentlen)
        {
            var cursor = FindPositionOfContentLengthOnSurface(consoleWriter.PromptTop, previousLen
                                                            , consoleWriter.BufferWidth);
            consoleWriter.SetCursorPosition(cursor.left, cursor.top);
            var diff = previousLen - currentlen;
            for (var i = diff; i > 0; i--)
                consoleWriter.Backspace();
        }

        static (int left, int top) FindPositionOfContentLengthOnSurface(
                int contentStartTop, int contentLen, int surfaceWidth)
        {
            var contentHeight = contentLen / surfaceWidth;

            var lenTop = contentStartTop + contentHeight;
            var lenLeft = contentLen % surfaceWidth;
            return (lenLeft, lenTop);
        }
    }

}
