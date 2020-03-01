using System;
using System.Threading.Tasks;
using Qmmands;

namespace Hanekawa.Shared.Command
{
    public abstract class HanekawaTypeParser<T> : TypeParser<T>
    {
        public override ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value,
            CommandContext context, IServiceProvider provider)
            => ParseAsync(parameter, value, (HanekawaContext)context, provider);

        public abstract ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value, HanekawaContext context,
            IServiceProvider provider);
    }
}
