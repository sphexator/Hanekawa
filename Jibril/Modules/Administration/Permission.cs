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

        [Group("add")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public class AddPermission : ModuleBase<SocketCommandContext>
        {
            private readonly LootCrates _loot;

            public AddPermission(LootCrates loot)
            {
                _loot = loot;
            }
            [Command("drop", RunMode = RunMode.Async)]
            public async Task AddDropChannel(ITextChannel channel = null)
            {
                try
                {
                    if (channel == null) channel = Context.Channel as ITextChannel;
                    await _loot.AddLootChannelAsync(channel as SocketTextChannel);
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Added {channel.Mention} to loot eligable channels.", Color.Green.RawValue)
                            .Build());
                }
                catch
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Couldn't add {channel.Mention} to loot eligable channels.", Color.Red.RawValue)
                            .Build());
                }
            }
        }

        [Group("remove")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public class RemovedPermission : ModuleBase<SocketCommandContext>
        {
            private readonly LootCrates _loot;

            public RemovedPermission(LootCrates loot)
            {
                _loot = loot;
            }

            [Command("drop", RunMode = RunMode.Async)]
            public async Task RemoveDropChannel(ITextChannel channel = null)
            {
                try
                {
                    if (channel == null) channel = Context.Channel as ITextChannel;
                    await _loot.RemoveLootChannelAsync(channel as SocketTextChannel);
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Removed {channel.Mention} from loot eligable channels.", Color.Green.RawValue)
                            .Build());
                }
                catch
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Couldn't remove {channel.Mention} from loot eligable channels.", Color.Red.RawValue)
                            .Build());
                }
            }
        }
    }
}
