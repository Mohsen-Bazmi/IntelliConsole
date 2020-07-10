using System.Linq;

namespace IntelliConsole
{
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

        ///*********
        int printedSuggestionsCount = 0;
        public void ClearPrintedSuggestions()
        {
            var originalTop = consoleWriter.CursorTop;
            var originalLeft = consoleWriter.CursorLeft;
            for (; printedSuggestionsCount > 0; printedSuggestionsCount--)
            {
                // consoleWriter.WriteLine();
                consoleWriter.CursorTop = originalTop + printedSuggestionsCount;
                consoleWriter.CursorLeft = consoleWriter.BufferWidth;
                for (var j = consoleWriter.BufferWidth; j > 0; j--)
                    // consoleWriter.Write(" ");
                    consoleWriter.Write("\b \b");
            }
            consoleWriter.CursorTop = originalTop;
            consoleWriter.CursorLeft = originalLeft;
        }



        public virtual void SuggestBasedOn(string currentLine)
        {
            var theLastWord = currentLine.Split(' ').Last();
            var matchedSuggestions = suggestions.ThatComplete(theLastWord);
            matchedSuggestions = matchedSuggestions.Select(m=>m.Replace(@"\r","").Replace("\n","")).ToArray();
            if (matchedSuggestions.Length == 0
            || consoleWriter.CursorLeft - theLastWord.Length < 0
            || matchedSuggestions.Any(s => s.Length + consoleWriter.CursorLeft
                                                    > consoleWriter.BufferWidth))
                return;

            var originalTop = consoleWriter.CursorTop;
            var originalLeft = consoleWriter.CursorLeft;
            var left = originalLeft - theLastWord.Length;
            consoleWriter.WriteLine();
            foreach (var suggestion in matchedSuggestions)
            {
                consoleWriter.CursorLeft = left;
                consoleWriter.WriteLine(suggestion);
            }
            printedSuggestionsCount = matchedSuggestions.Length + 1;
            var contentHeight = originalTop + matchedSuggestions.Length + 1;
            var windowHeight = consoleWriter.BufferHeight - 1;
            var overflowenSuggestionCount = contentHeight - windowHeight;

            if (overflowenSuggestionCount > 0)
            {
                consoleWriter.CursorTop -= overflowenSuggestionCount;
                // consoleWriter.CursorTop -= matchedSuggestions.Length + 1;
                promptTop -= overflowenSuggestionCount;
            }
            else
            {
                consoleWriter.CursorTop = originalTop;
            }
            consoleWriter.CursorLeft = originalLeft;
        }
        ///*********

        public void MoveToNextLine()
        {
            isInit = true;
            consoleWriter.WriteLine();
        }
        int previousLen = 0;
        int initialPromptTop;
        int promptTop;
        bool isInit = true;
        int promptLength;


        public void UpdateTheCurrentLine(string line, int localCursorLeft)
        {
            if (isInit)
            {
                isInit = false;
                previousLen = 0;
                promptLength = consoleWriter.CursorLeft;
                initialPromptTop = consoleWriter.CursorTop;
                promptTop = initialPromptTop;
            }
            var len = promptLength + line.Length;

            if (len == previousLen)
                System.Console.CursorVisible = false;

            ClearExtraCharacters(promptTop, previousLen, len);
            consoleWriter.SetCursorPosition(promptLength, promptTop);

            colourConsole.Write(line);


            consoleWriter.CursorLeft = (promptLength + localCursorLeft) % consoleWriter.BufferWidth;
            // consoleWriter.CursorTop=len/consoleWriter.BufferWidth;
            // var newLen = promptLength + line.Length;
            consoleWriter.CursorTop = promptTop + (promptLength + localCursorLeft) / consoleWriter.BufferWidth;

            if (len == previousLen)
                System.Console.CursorVisible = true;

            // if (line.Length > 0 && consoleWriter.CursorLeft == 0
            // && (len > previousLen))
            //     consoleWriter.CursorTop++;
            // if (consoleWriter.BufferWidth - 1 == consoleWriter.CursorLeft)
            //     if (newLen < previousLen)
            //         consoleWriter.CursorTop--;

            var lineHeight = len / consoleWriter.BufferWidth;
            var isLastLineOfConsole = promptTop + lineHeight >= consoleWriter.BufferHeight;
            // var isLastLineOfConsole = initialPromptTop == consoleWriter.BufferHeight - 1;

            if (isLastLineOfConsole && len >= previousLen)
            {
                promptTop = consoleWriter.BufferHeight - 1
                                - (len - 1) / consoleWriter.BufferWidth;

            }
            // if(consoleWriter.BufferWidth == consoleWriter.CursorLeft)
            //     currentPromptTop++;
            previousLen = len;
        }



        void ClearExtraCharacters(int promptTop, int previousLen, int currentlen)
        {
            var cursor = FindPositionOfContentLengthOnSurface(promptTop, previousLen
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

            var cursorTop = contentStartTop + contentHeight;
            var cursorLeft = contentLen % surfaceWidth;
            return (cursorLeft, cursorTop);
        }
    }

}
