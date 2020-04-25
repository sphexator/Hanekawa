using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Qmmands;

namespace Hanekawa.Shared.Command.TypeParser
{
    public class ColorTypeParser : HanekawaTypeParser<Color>
    {
        public static ColorTypeParser Instance => _instance ?? (_instance = new ColorTypeParser());

        private static ColorTypeParser _instance;

        private ColorTypeParser()
        { }

        public override ValueTask<TypeParserResult<Color>> ParseAsync(Parameter parameter, string value, HanekawaContext context, IServiceProvider provider)
        {
            var valueSpan = value.AsSpan();
            if (valueSpan.Length > 2)
            {
                var valid = false;
                if (valueSpan[0] == '0' && (valueSpan[1] == 'x' || valueSpan[1] == 'X') && valueSpan.Length == 8)
                {
                    valid = true;
                    valueSpan = valueSpan.Slice(2);
                }
                else if (value[0] == '#' && value.Length == 7)
                {
                    valid = true;
                    valueSpan = valueSpan.Slice(1);
                }

                if (valid && int.TryParse(valueSpan, NumberStyles.HexNumber, null, out var result))
                    return TypeParserResult<Color>.Successful(result);
            }

            return TypeParserResult<Color>.Unsuccessful("Invalid color hex value provided.");
        }
    }
}
