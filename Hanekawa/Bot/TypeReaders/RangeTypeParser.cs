using System.Threading.Tasks;
using Qmmands;

namespace Hanekawa.Bot.TypeReaders
{
    public class RangeTypeParser : TypeParser<Models.Range>
    {
        public override ValueTask<TypeParserResult<Models.Range>> ParseAsync(Parameter parameter, string value, CommandContext context)
        {
            var result = value.Split('-');
            int minValue = int.MaxValue;
            int maxValue = 0;
            foreach (var x in result)
            {
                if (!int.TryParse(x, out var val)) return TypeParserResult<Models.Range>.Unsuccessful("Failed to parse range");

                if (val > maxValue) maxValue = val;
                if (val < minValue) minValue = val;
            }

            return TypeParserResult<Models.Range>.Successful(new Models.Range(minValue, maxValue));
        }
    }
}
