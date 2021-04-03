namespace Hanekawa.Models
{
    public class Range
    {
        public Range(int minValue, int maxValue)
        {
            MaxValue = maxValue;
            MinValue = minValue;
        }

        public int MinValue { get; }
        public int MaxValue { get; }
    }
}