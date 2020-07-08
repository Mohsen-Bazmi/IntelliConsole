using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace IntelliConsole
{
    public interface ISyntaxHighlighting
    {
        ISyntaxHighlighting Char(char on, Paint color);
        ISyntaxHighlighting Regex(Regex rule, Paint color);
        ISyntaxHighlighting Word(string on, Paint color);
        ISyntaxHighlighting String(string on, Paint color);
        internal IntelliSenseViewModel[] GetAllRulesFor(Line context);
    }
}