using System;

namespace IntelliConsole
{
    public class TypingExperienceConfig
    {
        internal TypingExperienceConfig() { }
        Suggestions suggestions = Suggestions.From();
        ISyntaxHighlighting syntaxHighlighting = SyntaxHighlighting.Highlight;
        public ConsoleReader Console
        => new ConsoleReader(syntaxHighlighting, suggestions);

        public TypingExperienceConfig Suggest(params string[] suggestions)
        => Suggest(Suggestions.From(suggestions));
        public TypingExperienceConfig Suggest(Suggestions suggestions)
        {
            this.suggestions = suggestions;
            return this;
        }
        public TypingExperienceConfig Highlight(Func<ISyntaxHighlighting, ISyntaxHighlighting> f)
        {
            syntaxHighlighting = f(SyntaxHighlighting.Highlight);
            return this;
        }
    }
}