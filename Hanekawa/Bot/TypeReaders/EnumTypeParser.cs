using System;
using System.Threading.Tasks;
using Hanekawa.Core;
using Qmmands;

namespace Hanekawa.Bot.TypeReaders
{
    public class EnumTypeParser : HanekawaTypeParser<Enum>
    {
        public override ValueTask<TypeParserResult<Enum>> ParseAsync(Parameter parameter, string value,
            HanekawaContext context, IServiceProvider provider)
        {
            throw new NotImplementedException();
        }
    }
}