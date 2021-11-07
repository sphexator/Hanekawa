using System;
using System.Globalization;
using System.Threading.Tasks;
using Qmmands;

namespace Hanekawa.WebUI.Bot.Commands.TypeReaders
{
    public class TimeSpanTypeParser : TypeParser<TimeSpan>
    {
        private static readonly string[] Formats =
        {
            "%d'd'%h'h'%m'm'%s's'", //4d3h2m1s
            "%d'd'%h'h'%m'm'", //4d3h2m
            "%d'd'%h'h'%s's'", //4d3h  1s
            "%d'd'%h'h'", //4d3h
            "%d'd'%m'm'%s's'", //4d  2m1s
            "%d'd'%m'm'", //4d  2m
            "%d'd'%s's'", //4d    1s
            "%d'd'", //4d
            "%h'h'%m'm'%s's'", //  3h2m1s
            "%h'h'%m'm'", //  3h2m
            "%h'h'%s's'", //  3h  1s
            "%h'h'", //  3h
            "%m'm'%s's'", //    2m1s
            "%m'm'", //    2m
            "%s's'" //      1s
        };

        public override ValueTask<TypeParserResult<TimeSpan>> ParseAsync(Parameter parameter, string value,
            CommandContext context)
        {
            if (TimeSpan.TryParseExact(value.ToLowerInvariant(), Formats, CultureInfo.InvariantCulture,
                out var timespan)) return TypeParserResult<TimeSpan>.Successful(timespan);

            return !int.TryParse(value, out var minutes)
                ? TypeParserResult<TimeSpan>.Failed("Failed to parse timespan")
                : minutes <= 0
                    ? TypeParserResult<TimeSpan>.Failed("Failed to parse timespan")
                    : TypeParserResult<TimeSpan>.Successful(new TimeSpan(0, minutes, 0));
        }
    }
}
