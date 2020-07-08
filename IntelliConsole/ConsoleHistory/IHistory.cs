using System.Collections;
using System.Collections.Generic;

namespace IntelliConsole
{
    public interface IHistory : IEnumerable<string>
    {
        void Record(string line);
        IHistoryNarrator NewReverseNarrator();
    }
}