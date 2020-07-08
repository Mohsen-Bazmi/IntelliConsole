using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IntelliConsole
{
    public class History : IHistory
    {
        public static History Ancient => new History();

        readonly List<string> events = new List<string>();
        public int Length => events.Count;
        protected History() { }
        public string this[int index]
         => events[index];

        public IEnumerator<string> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        public IHistoryNarrator NewReverseNarrator()
        => new HistoryReverseNarrator(this);

        public void Record(string line)
        {
            if (!string.IsNullOrWhiteSpace(line))
                events.Add(line);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new System.NotImplementedException();
        }
    }
}