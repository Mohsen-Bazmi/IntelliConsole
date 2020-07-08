namespace IntelliConsole
{
    class LineContext
    {
        public static LineContext Initial
        => new LineContext(string.Empty, 0);
        protected LineContext(string displayingLine, int cursorLeft)
        {
            DisplayingLine = displayingLine;
            CursorLeft = cursorLeft;
        }
        public LineContext WithLine(string line)
        => new LineContext(line, CursorLeft);
        public LineContext WithCursorLeft(int left)
        => new LineContext(DisplayingLine, left);
        public string DisplayingLine { get; } = "";
        public int CursorLeft { get; } = 0;
    }
}
