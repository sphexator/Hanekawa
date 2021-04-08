using System.Threading.Tasks;
using Qmmands;

namespace Hanekawa.Bot.Commands.TypeReaders
{
    public class RangeTypeParser : TypeParser<Range>
    {
        public override ValueTask<TypeParserResult<Range>> ParseAsync(Parameter parameter, string value, CommandContext context)
        {
            var result = value.Split('-');
            int minValue = int.MaxValue;
            int maxValue = 0;
            foreach (var x in result)
            {
                if (!int.TryParse(x, out var val)) return TypeParserResult<Range>.Unsuccessful("Failed to parse range");

                if (val > maxValue) maxValue = val;
                if (val < minValue) minValue = val;
            }

            return TypeParserResult<Range>.Successful(new Range(minValue, maxValue));
        }
    }

    public class Range
    {
        public Range() { }
        public Range(int min, int max)
        {
            Max = max;
            Min = min;
        }
        
        public int Max { get; init; }
        public int Min { get; init; }
    }
}
