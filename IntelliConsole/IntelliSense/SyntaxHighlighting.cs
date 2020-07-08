using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace IntelliConsole
{

    //Applies all of the rules by the order they defined
    public class SyntaxHighlighting : ISyntaxHighlighting
    {
        List<Func<Line, IEnumerable<IntelliSenseViewModel>>> all = new List<Func<Line, IEnumerable<IntelliSenseViewModel>>>();

        IntelliSenseViewModel[] ISyntaxHighlighting.GetAllRulesFor(Line context)
        {
            return all.SelectMany(f => f(context)).Where(vm => vm.Length > 0).ToArray();
        }

        public ISyntaxHighlighting Regex(Regex rule, Paint color)
        => Apply(ctx =>
            rule.Matches(ctx.FullLine)
            .Select(match => new IntelliSenseViewModel(match.Index, match.Length, color)));

        public ISyntaxHighlighting Word(string on, Paint color)
        => Regex(new Regex("\\b" + System.Text.RegularExpressions.Regex.Escape(on) + "\\b"), color);

        public ISyntaxHighlighting String(string on, Paint color)
        => Regex(new Regex(System.Text.RegularExpressions.Regex.Escape(on)), color);
        public ISyntaxHighlighting Char(char on, Paint color)
        => String(char.ToString(on), color);
        public ISyntaxHighlighting Apply(Func<Line, IEnumerable<IntelliSenseViewModel>> rule)
        {
            all.Add(rule);
            return this;
        }
        public ISyntaxHighlighting If(Predicate<Line> predicate, Paint color)
        {
            throw new NotImplementedException();
        }

        public static SyntaxHighlighting Highlight
        => new SyntaxHighlighting();
        protected SyntaxHighlighting() { }

    }
}