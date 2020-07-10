using System;

namespace IntelliConsole
{
    class LineHandler
    {
        readonly IHistory history;
        IHistoryNarrator historyNarrator;
        ISuggestions suggestions;
        Writer writer;
        public LineHandler(IHistory history
                         , ISuggestions suggestions
                         , Writer writer)
        {
            this.history = history;
            historyNarrator = history.NewReverseNarrator();
            this.suggestions = suggestions;
            this.writer = writer;
        }
        public void NewLine()
        {
            writer.ClearPrintedSuggestions();
            history.Record(CurrentLine);

            context = LineContext.Initial;
            historyNarrator = history.NewReverseNarrator();
            writer.MoveToNextLine();
        }

        public string CurrentLine => context.DisplayingLine;
        LineContext context = LineContext.Initial;

        public virtual void Handle(ConsoleKeyInfo keyInfo)
        {
            writer.ClearPrintedSuggestions();

            var handler = new KeyHandler(historyNarrator, suggestions);
            context = handler.Handle(keyInfo, context);

            writer.UpdateTheCurrentLine(context.DisplayingLine, context.CursorLeft);
            writer.SuggestBasedOn(context.DisplayingLine);
        }
    }
}
