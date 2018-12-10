using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Addons.Database;
using Hanekawa.Extensions.Embed;
using Hanekawa.Services.CommandHandler;
using System.Threading.Tasks;

namespace Hanekawa.Modules.Permission
{
    [RequireContext(ContextType.Guild)]
    public class Permission : InteractiveBase
    {
        private readonly CommandHandlingService _command;

        public Permission(CommandHandlingService command)
        {
            _command = command;
        }

        [Command("permissions", RunMode = RunMode.Async)]
        [Alias("perm")]
        [Summary("Permission overview")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task ViewPermissionsAsync()
        {
            await Context.ReplyAsync("Currently disabled");
        }

        [Command("prefix", RunMode = RunMode.Async)]
        [Alias("set prefix")]
        [Summary("Sets custom prefix for this guild/server")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetPrefix(string prefix)
        {
            try
            {
                await _command.UpdatePrefixAsync(Context.Guild, prefix);
                await Context.ReplyAsync($"Successfully changed prefix to {prefix}", Color.Green.RawValue);
            }
            catch
            {
                await Context.ReplyAsync($"Something went wrong changing prefix to {prefix}",
                    Color.Red.RawValue);
            }
        }

        [Command("embed")]
        [Alias("set embed")]
        [Summary("Sets a custom colour for embeds")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetEmbed(uint color)
        {
            await Context.ReplyAsync("test", color);
        }
    }
}