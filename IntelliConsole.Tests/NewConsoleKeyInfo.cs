using System;

namespace IntelliConsole.Tests
{
    static class NewConsoleKeyInfo
    {
        public static ConsoleKeyInfo FromKey(ConsoleKey c)
        => new ConsoleKeyInfo((char)c, c, false, false, false);
        public static ConsoleKeyInfo FromChar(char c)
         => new ConsoleKeyInfo(c, (ConsoleKey)c, false, false, false);
    }
}