// using System;
// using System.Linq;

// namespace IntelliConsole
// {
//     class KeyHandler
//     {
//         readonly IConsoleWriter consoleWriter;
//         readonly IColourConsole colourConsole;
//         readonly IHistory history;
//         IHistoryNarrator historyNarrator;
//         ISuggestions suggestions;
//         public KeyHandler(IConsoleWriter consoleWriter
//                          , IHistory history
//                          , ISuggestions suggestions, IColourConsole colourConsole)
//         {
//             this.consoleWriter = consoleWriter;
//             this.history = history;
//             historyNarrator = history.NewReverseNarrator();
//             this.suggestions = suggestions;
//             this.colourConsole = colourConsole;
//         }
//         string currentLine = "";
//         int localCursorLeft = 0;
//         void ClearPrintedSuggestions()
//         {
//             var originalTop = consoleWriter.CursorTop;
//             var originalLeft = consoleWriter.CursorLeft;
//             for (; printedSuggestionsCount > 0; printedSuggestionsCount--)
//             {
//                 consoleWriter.CursorTop = originalTop + printedSuggestionsCount;
//                 consoleWriter.CursorLeft = consoleWriter.BufferWidth;
//                 for (var j = consoleWriter.BufferWidth; j > 0; j--)
//                     consoleWriter.Write("\b \b");
//             }
//             consoleWriter.CursorTop = originalTop;
//             consoleWriter.CursorLeft = originalLeft;
//         }
//         public virtual void Handle(ConsoleKeyInfo keyInfo)
//         {
//             ClearPrintedSuggestions();
//             switch (keyInfo.Key)
//             {
//                 case ConsoleKey.UpArrow:
//                     OnPreviousLine();
//                     break;
//                 case ConsoleKey.DownArrow:
//                     OnNextLine();
//                     break;
//                 case ConsoleKey.LeftArrow:
//                     OnLeftArrow();
//                     break;
//                 case ConsoleKey.B when keyInfo.Modifiers == ConsoleModifiers.Control:
//                     OnLeftArrow();
//                     break;
//                 case ConsoleKey.RightArrow:
//                     OnRightArrow();
//                     break;
//                 case ConsoleKey.End:
//                     OnEnd();
//                     break;
//                 case ConsoleKey.Home:
//                     OnHome();
//                     break;
//                 case ConsoleKey.A when keyInfo.Modifiers == ConsoleModifiers.Control:
//                     OnHome();
//                     break;
//                 case ConsoleKey.Tab:
//                     OnTab();
//                     break;
//                 case ConsoleKey.Backspace:
//                     OnBackspace();
//                     break;
//                 case ConsoleKey.Delete:
//                     OnDelete();
//                     break;
//                 default:
//                     RecordChar(keyInfo.KeyChar);
//                     break;
//             }
//             Suggest();
//         }



//         protected virtual void OnHome()
//         {
//             PrintLine(currentLine, 0);
//         }
//         protected virtual void OnEnd()
//         {
//             PrintLine(currentLine, currentLine.Length);
//         }

//         protected virtual void OnTab()
//         {
//             var lineWords = currentLine.Split(' ');
//             var matchedSuggestions = suggestions.ThatComplete(lineWords.Last());
//             if (matchedSuggestions.Length == 0)
//                 return;

//             consoleWriter.WriteLine();
//             var previousWords = string.Join(' ', lineWords.SkipLast(1));
//             if (lineWords.Length > 1)
//                 previousWords += ' ';
//             currentLine = previousWords + matchedSuggestions[0];
//             PrintLine(currentLine, localCursorLeft: currentLine.Length);
//         }
//         int printedSuggestionsCount = 0;
//         protected virtual void Suggest()
//         {
//             var theLastWord = currentLine.Split(' ').Last();
//             var matchedSuggestions = suggestions.ThatComplete(theLastWord);
//             if (matchedSuggestions.Length == 0)
//                 return;

//             var originalTop = consoleWriter.CursorTop;
//             var originalLeft = consoleWriter.CursorLeft;
//             consoleWriter.WriteLine();
//             foreach (var suggestion in matchedSuggestions)
//             {
//                 consoleWriter.CursorLeft = originalLeft - theLastWord.Length;
//                 consoleWriter.WriteLine(suggestion);
//             }
//             printedSuggestionsCount = matchedSuggestions.Length;
//             if (originalTop + matchedSuggestions.Length > consoleWriter.BufferHeight - 1)
//             {
//                 consoleWriter.CursorTop -= matchedSuggestions.Length + 1;


//                 var contentHeight = originalTop + matchedSuggestions.Length + 1;
//                 var windowHeight = consoleWriter.BufferHeight - 1;
//                 var diff = contentHeight - windowHeight;
//                 promptTop -= diff;
//             }
//             else
//             {
//                 consoleWriter.CursorTop = originalTop;
//             }
//             consoleWriter.CursorLeft = originalLeft;
//         }

//         protected virtual void OnBackspace()
//         {
//             if (string.IsNullOrEmpty(currentLine))
//                 return;
//             if (localCursorLeft == 0)
//                 return;
//             currentLine = currentLine[0..(localCursorLeft - 1)]
//                         + currentLine[localCursorLeft..];
//             PrintLine(currentLine, localCursorLeft - 1);
//         }
//         protected virtual void OnDelete()
//         {
//             if (localCursorLeft == currentLine.Length)
//                 return;
//             currentLine = currentLine[0..localCursorLeft]
//                         + currentLine[(localCursorLeft + 1)..];
//             PrintLine(currentLine, localCursorLeft);
//         }

//         protected virtual void OnLeftArrow()
//         {
//             if (localCursorLeft == 0)
//                 return;
//             if (consoleWriter.CursorLeft == 0)
//             {
//                 consoleWriter.CursorTop--;
//                 consoleWriter.CursorLeft = consoleWriter.BufferWidth;
//                 return;
//             }
//             localCursorLeft--;
//             consoleWriter.CursorLeft--;
//         }

//         protected virtual void OnRightArrow()
//         {
//             if (consoleWriter.CursorLeft == consoleWriter.BufferWidth)
//             {
//                 consoleWriter.CursorLeft = 0;
//                 consoleWriter.CursorTop++;
//                 return;
//             }
//             localCursorLeft++;
//             currentLine += ' ';
//             consoleWriter.CursorLeft++;
//         }


//         protected virtual bool IsCharAllowed(char c)
//         => c != '\0';

//         protected virtual void RecordChar(char c)
//         {
//             if (!IsCharAllowed(c))
//                 return;
//             var str = char.ToString(c);
//             if (string.IsNullOrEmpty(str))
//                 return;

//             try
//             {
//                 currentLine = currentLine.Insert(localCursorLeft, char.ToString(c));
//             }
//             catch (ArgumentOutOfRangeException ex)
//             {
//                 throw new ArgumentOutOfRangeException($@"
// cursorLeft:{localCursorLeft},line:{currentLine},char:{c}", ex);
//             }
//             PrintLine(currentLine, localCursorLeft + str.Length);
//         }

//         void PrintLine(string state, int localCursorLeft)
//         {
//             this.localCursorLeft = localCursorLeft;
//             UpdateTheCurrentLine(state);
//         }

//         protected virtual void OnPreviousLine()
//         {
//             if (!historyNarrator.MovePrevious())
//                 return;
//             currentLine = historyNarrator.Current;
//             PrintLine(currentLine, localCursorLeft: currentLine.Length);
//         }
//         protected virtual void OnNextLine()
//         {
//             if (!historyNarrator.MoveNext())
//             {
//                 return;
//             }
//             currentLine = historyNarrator.Current;
//             PrintLine(currentLine, localCursorLeft: currentLine.Length);
//         }

//         void Flush()
//         {
//             currentLine = "";
//             localCursorLeft = 0;
//             historyNarrator = history.NewReverseNarrator();
//         }
        
//         public virtual string Finalize()
//         {
//             var finalState = currentLine;
//             isInit = true;
//             consoleWriter.WriteLine();
//             history.Record(finalState);
//             Flush();
//             return finalState;
//         }
//         int previousLen = 0;
//         int initialPromptTop;
//         int promptTop;
//         bool isInit = true;
//         int promptLength;


//         void UpdateTheCurrentLine(string line)
//         {
//             if (isInit)
//             {
//                 isInit = false;
//                 previousLen = 0;
//                 promptLength = consoleWriter.CursorLeft;
//                 initialPromptTop = consoleWriter.CursorTop;
//                 promptTop = initialPromptTop;
//             }
//             var len = promptLength + line.Length;


//             ClearExtraCharacters(promptTop, previousLen, len);
//             consoleWriter.SetCursorPosition(promptLength, promptTop);

//             colourConsole.Write(line);


//             consoleWriter.CursorLeft = (promptLength + localCursorLeft) % consoleWriter.BufferWidth;
//             // consoleWriter.CursorTop=len/consoleWriter.BufferWidth;
//             // var newLen = promptLength + line.Length;
//             consoleWriter.CursorTop = promptTop + (promptLength + localCursorLeft) / consoleWriter.BufferWidth;
//             // if (line.Length > 0 && consoleWriter.CursorLeft == 0
//             // && (len > previousLen))
//             //     consoleWriter.CursorTop++;
//             // if (consoleWriter.BufferWidth - 1 == consoleWriter.CursorLeft)
//             //     if (newLen < previousLen)
//             //         consoleWriter.CursorTop--;

//             var lineHeight = len / consoleWriter.BufferWidth;
//             var isLastLineOfConsole = promptTop + lineHeight >= consoleWriter.BufferHeight;
//             // var isLastLineOfConsole = initialPromptTop == consoleWriter.BufferHeight - 1;

//             if (isLastLineOfConsole && len >= previousLen)
//             {
//                 promptTop = consoleWriter.BufferHeight - 1
//                                 - (len - 1) / consoleWriter.BufferWidth;

//             }
//             // if(consoleWriter.BufferWidth == consoleWriter.CursorLeft)
//             //     currentPromptTop++;
//             previousLen = len;
//         }



//         void ClearExtraCharacters(int promptTop, int previousLen, int currentlen)
//         {
//             var cursor = FindPositionOfContentLengthOnSurface(promptTop, previousLen
//                                                             , consoleWriter.BufferWidth);
//             consoleWriter.SetCursorPosition(cursor.left, cursor.top);
//             var diff = previousLen - currentlen;
//             for (var i = diff; i > 0; i--)
//                 consoleWriter.Backspace();
//         }

//         static (int left, int top) FindPositionOfContentLengthOnSurface(
//                 int contentStartTop, int contentLen, int surfaceWidth)
//         {
//             var contentHeight = contentLen / surfaceWidth;

//             var cursorTop = contentStartTop + contentHeight;
//             var cursorLeft = contentLen % surfaceWidth;
//             return (cursorLeft, cursorTop);
//         }

//     }
// }
