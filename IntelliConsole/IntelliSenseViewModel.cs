namespace IntelliConsole
{
    public class IntelliSenseViewModel
    {
        public IntelliSenseViewModel(int fromIndex, int length, Paint color)
        {
            FromIndex = fromIndex;
            Length = length;
            Color = color;
        }
        public int FromIndex { get; }
        public int Length { get; }
        public Paint Color { get; }
    }
}
