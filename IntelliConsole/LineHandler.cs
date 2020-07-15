using System;
using System.Reactive.Linq;

namespace IntelliConsole
{
    class LineHandler
    {
        readonly IHistory history;
        IHistoryNarrator historyNarrator;
        readonly ISuggestions suggestions;
        readonly SuggestionPrinter suggestionPrinter;
        readonly Writer writer;
        public LineHandler(IHistory history
                         , ISuggestions suggestions
                         , Writer writer
                         , SuggestionPrinter suggestionPrinter)
        {
            this.history = history;
            historyNarrator = history.NewReverseNarrator();
            this.suggestions = suggestions;
            this.writer = writer;
            this.suggestionPrinter = suggestionPrinter;
        }
        public void NewLine()
        {
            suggestionPrinter.ClearPrintedSuggestions();
            history.Record(CurrentLine);

            context = LineContext.Initial;
            historyNarrator = history.NewReverseNarrator();
            writer.MoveToNextLine();
        }

        public string CurrentLine => context.DisplayingLine;
        LineContext context = LineContext.Initial;

        // public virtual IObservable<string> HandleKeyAndNotifyLines(ConsoleKeyInfo ki)
        // {
        //     if (ki.Key == ConsoleKey.Enter)
        //     {
        //         var result = Observable.Return(CurrentLine);
        //         NewLine();
        //         return result;
        //     }
        //     Handle(ki);
        //     return Observable.Empty<string>();
        // }
        public virtual void Handle(ConsoleKeyInfo keyInfo)
        {
            if (suggestionPrinter.CanHandle(keyInfo))
            {
                context = suggestionPrinter.Handle(keyInfo, context);

                writer.UpdateTheCurrentLine(context.DisplayingLine, context.CursorLeft);
                // suggestionPrinter.SuggestBasedOn(context.DisplayingLine);
            }
            else
            {
                suggestionPrinter.ClearPrintedSuggestions();
                var handler = new KeyHandler(historyNarrator, suggestions);
                context = handler.Handle(keyInfo, context);
                writer.UpdateTheCurrentLine(context.DisplayingLine, context.CursorLeft);
                suggestionPrinter.SuggestBasedOn(context.DisplayingLine);
            }
        }
    }
}
