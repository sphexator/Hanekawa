﻿using System.Threading.Tasks;
using Discord;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Bot.Services.Command;
using Hanekawa.Core.Interactive;
using Hanekawa.Database;
using Hanekawa.Extensions.Embed;
using Qmmands;

namespace Hanekawa.Bot.Modules.Settings
{
    [Name("Settings")]
    [Description("Server settings")]
    [RequireBotPermission(GuildPermission.EmbedLinks)]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    public class Settings : InteractiveBase
    {
        private readonly CommandHandlingService _command;
        public Settings(CommandHandlingService command) => _command = command;

        [Name("Add prefix")]
        [Command("add prefix", "aprefix")]
        [Description("Adds a prefix to the bot, if it doesn't already exist")]
        [Remarks("add prefix !")]
        public async Task AddPrefixAsync([Remainder]string prefix)
        {
            using (var db = new DbService())
            {
                if (await _command.AddPrefix(Context.Guild.Id, prefix, db))
                {
                    await Context.ReplyAsync($"Added {prefix} as a prefix.", Color.Green.RawValue);
                }
                else await Context.ReplyAsync($"{prefix} is already a prefix on this server.", Color.Red.RawValue);
            }
        }

        [Name("Remove prefix")]
        [Command("remove prefix", "rprefix")]
        [Description("Removes a prefix from the bot, if it exists")]
        [Remarks("remove prefix !")]
        public async Task RemovePrefixAsync([Remainder]string prefix)
        {
            using (var db = new DbService())
            {
                if (await _command.RemovePrefix(Context.Guild.Id, prefix, db))
                {
                    await Context.ReplyAsync($"Added {prefix} as a prefix.", Color.Green.RawValue);
                }
                else await Context.ReplyAsync($"{prefix} is already a prefix on this server.", Color.Red.RawValue);
            }
        }

        [Name("Set embed color")]
        [Command("embed", "set embed")]
        [Description("Changes the embed colour of the bot")]
        [Remarks("embed 346423")]
        public async Task SetEmbedColorAsync(uint color)
        {
            // TODO: Embed color commands
        }
    }
}