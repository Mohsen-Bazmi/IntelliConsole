using System;

namespace IntelliConsole
{
    public class Paint
    {
        public Paint(ConsoleColor? foregroundColor = null, ConsoleColor? backgroundColor = null)
        {
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
        }
        internal ConsoleColor? ForegroundColor = null;
        internal ConsoleColor? BackgroundColor = null;
        public static Paint Foreground(ConsoleColor color)
        => new Paint((ConsoleColor?)color);
        public static Paint Background(ConsoleColor color)
        => new Paint(null, (ConsoleColor?)color);
        public Paint AndFore(ConsoleColor color)
        => new Paint((ConsoleColor?)color);
        public Paint AndBack(ConsoleColor color)
        => new Paint(null, (ConsoleColor?)color);
    }
}