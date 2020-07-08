using System.Collections;
using System.Collections.Generic;

namespace IntelliConsole
{

    public interface IHistoryNarrator : IEnumerator<string>
    {
        bool MovePrevious();
    }
}