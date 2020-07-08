using System.Linq;

namespace IntelliConsole
{
    public interface IColourConsole
    {
        void Write(string text);
    }
    public class ColourConsoleWriter : IColourConsole
    {
        readonly ISyntaxHighlighting intelliSense;
        readonly IConsoleWriter consoleWriter;
        public ColourConsoleWriter(ISyntaxHighlighting intelliSense, IConsoleWriter consoleWriter)
        {
            this.intelliSense = intelliSense;
            this.consoleWriter = consoleWriter;
        }
        public void Write(string text)
        {
            var viewModels = intelliSense.GetAllRulesFor(new Line(text));

            var defaultBackgroundColor = consoleWriter.BackgroundColor;
            var defaultForegroundColor = consoleWriter.ForegroundColor;
            for (var i = 0; i < text.Length; i++)
            {
                var vm = viewModels.LastOrDefault(vm => vm.FromIndex <= i && i < vm.FromIndex + vm.Length);
                if (vm != null)
                {
                    consoleWriter.BackgroundColor = vm.Color.BackgroundColor ?? defaultBackgroundColor;
                    consoleWriter.ForegroundColor = vm.Color.ForegroundColor ?? defaultForegroundColor;
                }
                consoleWriter.Write(text[i]);
                consoleWriter.BackgroundColor = defaultBackgroundColor;
                consoleWriter.ForegroundColor = defaultForegroundColor;
            }
        }
    }
}