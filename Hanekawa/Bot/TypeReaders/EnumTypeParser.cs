using System;
using System.Threading.Tasks;
using Qmmands;

namespace Hanekawa.Bot.TypeReaders
{
    public class EnumTypeParser : TypeParser<Enum>
    {
        public override ValueTask<TypeParserResult<Enum>> ParseAsync(Parameter parameter, string value, CommandContext context) => throw new NotImplementedException();
    }
}