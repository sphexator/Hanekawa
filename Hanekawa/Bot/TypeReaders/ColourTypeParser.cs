using System;
using System.Globalization;
using System.Threading.Tasks;
using Disqord;
using Hanekawa.Shared.Command;
using Qmmands;

namespace Hanekawa.Bot.TypeReaders
{
    public class ColourTypeParser : TypeParser<Color>
    {
        public override ValueTask<TypeParserResult<Color>> ParseAsync(Parameter parameter, string value, CommandContext context)
        {
            if(Enum.TryParse(typeof(HanaColor), value, true, out var enumResult))
            {
                if (enumResult is HanaColor color)
                {
                    switch (color)
                    {
                        case HanaColor.Default:
                            return TypeParserResult<Color>.Successful(HanaBaseColor.Default());
                        case HanaColor.Green:
                            return TypeParserResult<Color>.Successful(HanaBaseColor.Green());
                        case HanaColor.Red:
                            return TypeParserResult<Color>.Successful(HanaBaseColor.Red());
                        case HanaColor.Blue:
                            return TypeParserResult<Color>.Successful(HanaBaseColor.Blue());
                        case HanaColor.Purple:
                            return TypeParserResult<Color>.Successful(HanaBaseColor.Purple());
                        case HanaColor.Pink:
                            return TypeParserResult<Color>.Successful(HanaBaseColor.Pink());
                        case HanaColor.Yellow:
                            return TypeParserResult<Color>.Successful(HanaBaseColor.Yellow());
                        case HanaColor.Black:
                            return TypeParserResult<Color>.Successful(HanaBaseColor.Black());
                        case HanaColor.White:
                            return TypeParserResult<Color>.Successful(HanaBaseColor.White());
                        case HanaColor.Brown:
                            return TypeParserResult<Color>.Successful(HanaBaseColor.Brown());
                        case HanaColor.Orange:
                            return TypeParserResult<Color>.Successful(HanaBaseColor.Orange());
                        case HanaColor.Aqua:
                            return TypeParserResult<Color>.Successful(HanaBaseColor.Aqua());
                        case HanaColor.Maroon:
                            return TypeParserResult<Color>.Successful(HanaBaseColor.Maroon());
                        case HanaColor.Olive:
                            return TypeParserResult<Color>.Successful(HanaBaseColor.Olive());
                        case HanaColor.Gray:
                            return TypeParserResult<Color>.Successful(HanaBaseColor.Gray());
                        case HanaColor.Silver:
                            return TypeParserResult<Color>.Successful(HanaBaseColor.Silver());
                        case HanaColor.Fuchsia:
                            return TypeParserResult<Color>.Successful(HanaBaseColor.Fuchsia());
                        case HanaColor.Navy:
                            return TypeParserResult<Color>.Successful(HanaBaseColor.Navy());
                        case HanaColor.Teal:
                            return TypeParserResult<Color>.Successful(HanaBaseColor.Teal());
                        case HanaColor.Lime:
                            return TypeParserResult<Color>.Successful(HanaBaseColor.Lime());
                    }
                }
            }
            if (value.Length > 2)
            {
                var s = value.AsSpan();
                var flag = false;
                if (s[0] == '0' && (s[1] == 'x' || s[1] == 'X') && s.Length == 8)
                {
                    flag = true;
                    s = s.Slice(2);
                }
                else if (value[0] == '#' && value.Length == 7)
                {
                    flag = true;
                    s = s.Slice(1);
                }

                if (flag && uint.TryParse(s, NumberStyles.HexNumber, null, out var result))
                    return TypeParserResult<Color>.Successful((int)result);
            }
            return TypeParserResult<Color>.Unsuccessful("Invalid color name or hex value.");
        }
    }
}
