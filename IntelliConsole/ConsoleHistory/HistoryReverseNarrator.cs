using System.Collections;

namespace IntelliConsole
{
    public class HistoryReverseNarrator : IHistoryNarrator
    {
        readonly History history;
        int currentIndex;
        public HistoryReverseNarrator(History history)
        {
            this.history = history;
            currentIndex = history.Length;
        }
        public string Current { get; private set; }



        public bool MovePrevious()
        {
            if (currentIndex == 0)
                return false;
            Current = history[--currentIndex];
            return true;
        }
        public bool MoveNext()
        {
            if (currentIndex >= history.Length - 1)
                return false;
            Current = history[++currentIndex];
            return true;
        }


        object IEnumerator.Current => Current;
        public void Reset()
        {
            throw new System.NotImplementedException();
        }
        public void Dispose()
        {
        }
    }
}