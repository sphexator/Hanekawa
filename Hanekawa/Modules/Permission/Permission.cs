using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Extensions;
using Hanekawa.Services.CommandHandler;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Hanekawa.Extensions.Embed;

namespace Hanekawa.Modules.Permission
{
    [RequireContext(ContextType.Guild)]
    public class Permission : InteractiveBase
    {
        private readonly CommandHandlingService _command;
        private readonly DbService _db;

        public Permission(CommandHandlingService command, DbService db)
        {
            _command = command;
            _db = db;
        }

        [Command("permissions", RunMode = RunMode.Async)]
        [Alias("perm")]
        [Summary("Permission overview")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task ViewPermissionsAsync()
        {
            await Context.ReplyAsync("Currently disabled");
        }

        [Command("set prefix", RunMode = RunMode.Async)]
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
    }
}