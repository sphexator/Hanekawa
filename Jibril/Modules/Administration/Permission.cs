using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Extensions;
using Jibril.Services;
using Jibril.Services.Loot;
using System.Threading.Tasks;

namespace Jibril.Modules.Administration
{
    public class Permission : ModuleBase<SocketCommandContext>
    {
        [Group("set")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public class SetPermission : ModuleBase<SocketCommandContext>
        {
            private readonly CommandHandlingService _command;
            public SetPermission(CommandHandlingService command)
            {
                _command = command;
            }

            [Command("prefix", RunMode = RunMode.Async)]
            public async Task SetPrefix(string prefix)
            {
                try
                {
                    _command.UpdatePrefix(Context.Guild.Id, prefix);
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Successfully changed prefix to {prefix}!", Color.Green.RawValue).Build());
                }
                catch
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Something went wrong changing prefix to {prefix}...", Color.Red.RawValue).Build());
                }
            }
        }

        [Group("drop")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public class AddPermission : ModuleBase<SocketCommandContext>
        {
            private readonly LootCrates _loot;

            public AddPermission(LootCrates loot)
            {
                _loot = loot;
            }
        }
    }
}