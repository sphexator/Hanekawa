using Hanekawa.Shared.Command.Extensions;

namespace Hanekawa.Shared.Command
{
    public class RoslynCommandContext
    {
        public HanekawaCommandContext Context { get; }

        public RoslynCommandContext(HanekawaCommandContext context)
        {
            Context = context;
        }

        public string Inspect(object obj)
        {
            return obj.Inspect();
        }
    }
}
