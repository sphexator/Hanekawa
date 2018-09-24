using System;
using System.Threading.Tasks;
using Discord.Commands;
using Hanekawa.Extensions;
using Hanekawa.Services.Entities;

namespace Hanekawa.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireMusicChannel : RequireContextAttribute
    {
        public RequireMusicChannel() : base(ContextType.Guild)
        {
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context,
            CommandInfo command, IServiceProvider services)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfig(context.Guild);
                if (!cfg.MusicChannel.HasValue) return PreconditionResult.FromSuccess();
                return context.Channel.Id != cfg.MusicChannel.Value
                    ? PreconditionResult.FromError("Wrong channel")
                    : PreconditionResult.FromSuccess();
            }
        }
    }
}