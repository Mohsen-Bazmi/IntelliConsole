using System;
using System.Linq;

namespace IntelliConsole
{
    public interface ISuggestions
    {
        string[] ThatComplete(string start);
    }
    public class Suggestions : ISuggestions
    {
        public static Suggestions From(params string[] possibleSuggestions)
        => new Suggestions(possibleSuggestions);
        string[] possibleSuggestions;
        protected Suggestions(string[] possibleSuggestions)
        {
            this.possibleSuggestions = possibleSuggestions;
        }
        public string[] ThatComplete(string start)
        {
            if (string.IsNullOrWhiteSpace(start))
                return Array.Empty<string>();
            var result = possibleSuggestions
                            .Where(s => s.StartsWith(start, StringComparison.OrdinalIgnoreCase))
                            .ToArray();
            return result;
        }

    }

}