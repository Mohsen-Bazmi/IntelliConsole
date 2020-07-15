using System;
using System.Reactive.Linq;

namespace IntelliConsole
{
    public class ConsoleReader
    {
        readonly IConsoleObserver consoleObserver;
        readonly LineHandler lineHandler;
        public static TypingExperienceConfig ConfigTypingExperience
        => new TypingExperienceConfig();
        internal ConsoleReader(ISyntaxHighlighting intelliSense, ISuggestions suggestions)
        : this(new ConsoleProxy(), new ConsoleProxy()
              , intelliSense
              , History.Ancient
              , suggestions)
        { }
        public static ConsoleReader Default
        => new ConsoleReader();
        protected ConsoleReader() : this(SyntaxHighlighting.Highlight, Suggestions.From()) { }
        public ConsoleReader(IConsoleObserver consoleObserver
                            , IConsoleWriter consoleWriter
                            , ISyntaxHighlighting intelliSense
                            , IHistory history
                            , ISuggestions suggestions)
        {
            this.consoleObserver = consoleObserver;
            var colourConsole = new ColourConsoleWriter(intelliSense, consoleWriter);
            var writer = new Writer(consoleWriter, colourConsole, suggestions);
            var suggestionPrinter = new SuggestionPrinter(consoleWriter, colourConsole, suggestions);
            lineHandler = new LineHandler(history, suggestions, writer,suggestionPrinter);
        }

        public IObservable<string> ObserveLines()
        => consoleObserver.KeyPress.SelectMany(ki =>
         {
             //  return lineHandler.HandleKeyAndNotifyLines(ki);
             if (ki.Key == ConsoleKey.Enter)
             {
                 var line = lineHandler.CurrentLine;
                 lineHandler.NewLine();
                 return Observable.Return(line);
             }
             lineHandler.Handle(ki);
             return Observable.Empty<string>();
         });
        // => consoleObserver.KeyPress
        //    .TakeUntil(info => info.Key == ConsoleKey.Enter)
        //    .Select(keyHandler.Handle)
        // //    .LastOrDefaultAsync()
        //    ;

        public string ReadLine()
        => ObserveLines().Take(1).Wait();
    }
}