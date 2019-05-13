using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;

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
            if (context.User is SocketGuildUser user && user.GuildPermissions.ManageGuild)
                return PreconditionResult.FromSuccess();
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfigAsync(context.Guild);
                if (!cfg.MusicChannel.HasValue) return PreconditionResult.FromSuccess();
                return context.Channel.Id != cfg.MusicChannel.Value
                    ? PreconditionResult.FromError("Wrong channel")
                    : PreconditionResult.FromSuccess();
            }
        }
    }
}