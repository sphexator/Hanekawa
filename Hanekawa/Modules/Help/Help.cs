using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Extensions.Embed;
using Hanekawa.Preconditions;
using Hanekawa.Services.CommandHandler;
using Quartz.Util;

namespace Hanekawa.Modules.Help
{
    public class Help : InteractiveBase
    {
        private readonly CommandHandlingService _commandHandling;
        private readonly CommandService _commands;
        private readonly IServiceProvider _map;

        public Help(IServiceProvider map, CommandService commands, CommandHandlingService commandHandling)
        {
            _commands = commands;
            _commandHandling = commandHandling;
            _map = map;
        }

        [Command("help")]
        [Summary("Lists this bots commands.")]
        [Ratelimit(1, 2, Measure.Seconds)]
        [Priority(1)]
        [RequiredChannel]
        public async Task HelpAsync([Remainder] string path = "")
        {
            var prefix = _commandHandling.GetPrefix(Context.Guild.Id);
            if (path == "")
            {
                var content = GetModules();
                var embed = new EmbedBuilder()
                    .CreateDefault(content, Context.Guild.Id)
                    .WithAuthor(new EmbedAuthorBuilder {Name = "Module list"})
                    .WithFooter(
                        new EmbedFooterBuilder {Text = $"Use `{prefix}help <module>` to get help with a module"});
                await Context.ReplyAsync(embed);
            }
            else
            {
                var mod = _commands.Modules.FirstOrDefault(
                    m => string.Equals(m.Name.Replace("Module", "").ToLower(), path.ToLower(),
                        StringComparison.CurrentCultureIgnoreCase));
                if (mod == null || mod.Name == "test" || mod.Name == "owner")
                {
                    await Context.ReplyAsync("No module could be found with that name.\n" +
                                             "Modules:\n" +
                                             $"{GetModules()}", Color.Red.RawValue);
                    return;
                }

                var pages = GetCommands(mod, prefix).PaginateBuilder(Context.Guild.Id, $"Commands for {mod.Name}");
                await PagedReplyAsync(pages);
            }
        }

        private string GetModules()
        {
            string result = null;
            var modules = _commands.Modules.ToList();
            for (var i = 0; i < modules.Count;)
            {
                var stringBuilder = new StringBuilder();
                for (var j = 0; j < 5; j++)
                {
                    if (i >= modules.Count) continue;
                    var x = modules[i];
                    if (x.Name == "test" || x.Name == "owner")
                    {
                        j--;
                        i++;
                        continue;
                    }

                    stringBuilder.Append(j < 4 ? $"`{x.Name}` - " : $"`{x.Name}`");
                    i++;
                }

                result += $"{stringBuilder}\n";
            }

            return result;
        }

        private List<string> GetCommands(ModuleInfo module, string prefix)
        {
            var result = new List<string>();
            foreach (var x in module.Commands)
            {
                var perm = GetPermissions(x);
                var content = new StringBuilder();
                if (!x.Name.IsNullOrWhiteSpace()) content.AppendLine($"**{x.Name}**");
                if (!perm.IsNullOrWhiteSpace()) content.Append($"{perm}");
                if (!x.Summary.IsNullOrWhiteSpace()) content.AppendLine($"{x.Summary}");
                content.AppendLine($"Usage: **{prefix}{GetPrefix(x)} {ParamBuilder(x)}**");
                if (!x.Remarks.IsNullOrWhiteSpace()) content.AppendLine($"Example: **{x.Remarks}**");
                result.Add(content.ToString());
            }

            return result;
        }

        private string ParamBuilder(CommandInfo command)
        {
            var output = new StringBuilder();
            if (!command.Parameters.Any()) return output.ToString();
            foreach (var x in command.Parameters)
                if (x.IsOptional)
                    output.Append($"[{x.Name} = {x.DefaultValue}] ");
                else if (x.IsRemainder)
                    output.Append($"...{x.Name} ");
                else if (x.IsMultiple)
                    output.Append($"|{x.Name}| ");
                else
                    output.Append($"<{x.Name}> ");

            return output.ToString();
        }

        private string GetPrefix(CommandInfo command)
        {
            var output = GetPrefix(command.Module);
            output += $"{command.Aliases.FirstOrDefault()} ";
            return output;
        }

        private string GetPrefix(ModuleInfo module)
        {
            var output = "";
            if (module.Parent != null) output = $"{GetPrefix(module.Parent)}{output}";
            return output;
        }

        private string GetPermissions(CommandInfo cmd)
        {
            var result = new StringBuilder();
            foreach (var x in cmd.Module.Preconditions)
            {
                if (x is RequireUserPermissionAttribute perm)
                {
                    if (perm.GuildPermission.HasValue)
                        result.AppendLine(GuildPermissionString(perm.GuildPermission.Value));
                    if (perm.ChannelPermission.HasValue)
                        result.AppendLine(ChannelPermissionString(perm.ChannelPermission.Value));
                }

                if (x is RequireOwnerAttribute) result.AppendLine("**Require Bot Owner**");
            }

            foreach (var x in cmd.Preconditions)
            {
                if (x is RequireUserPermissionAttribute perm)
                {
                    if (perm.GuildPermission.HasValue)
                        result.AppendLine(GuildPermissionString(perm.GuildPermission.Value));
                    if (perm.ChannelPermission.HasValue)
                        result.AppendLine(ChannelPermissionString(perm.ChannelPermission.Value));
                }

                if (x is RequireOwnerAttribute) result.AppendLine("**Require Bot Owner**");
                if (x is WhiteListedDesigner) result.AppendLine("**Require Whitelisted Designer**");
                if (x is WhiteListedEventOrg) result.AppendLine("**Require Whitelisted Event Organizer**");
                if (x is WhiteListedOverAll) result.AppendLine("**Require Whitelisted Member**");
            }

            return result.Length == 0 ? null : result.ToString();
        }

        private string GuildPermissionString(GuildPermission perm)
        {
            string result = null;
            switch (perm)
            {
                case GuildPermission.CreateInstantInvite:
                    result = "**Require Instant Invite**";
                    break;
                case GuildPermission.KickMembers:
                    result = "**Require Kick Members**";
                    break;
                case GuildPermission.BanMembers:
                    result = "**Require Ban Members**";
                    break;
                case GuildPermission.Administrator:
                    result = "**Require Administrator**";
                    break;
                case GuildPermission.ManageChannels:
                    result = "**Require Manage Channels**";
                    break;
                case GuildPermission.ManageGuild:
                    result = "**Require Manage Server**";
                    break;
                case GuildPermission.AddReactions:
                    result = "**Require Add Reaction**";
                    break;
                case GuildPermission.ViewAuditLog:
                    result = "**Require View Audit Log**";
                    break;
                case GuildPermission.ViewChannel:
                    result = "**Require View Messages**";
                    break;
                case GuildPermission.SendMessages:
                    result = "**Require Send Messages**";
                    break;
                case GuildPermission.SendTTSMessages:
                    result = "**Require Send TTS Messages**";
                    break;
                case GuildPermission.ManageMessages:
                    result = "**Require Manage Messages**";
                    break;
                case GuildPermission.EmbedLinks:
                    result = "**Require Embed Links**";
                    break;
                case GuildPermission.AttachFiles:
                    result = "**Require Attach Files**";
                    break;
                case GuildPermission.ReadMessageHistory:
                    result = "**Require Read Message History**";
                    break;
                case GuildPermission.MentionEveryone:
                    result = "**Require Mention Everyone**";
                    break;
                case GuildPermission.UseExternalEmojis:
                    result = "**Require Use External Emojis**";
                    break;
                case GuildPermission.Connect:
                    result = "**Require Voice Connect**";
                    break;
                case GuildPermission.Speak:
                    result = "**Require Voice Speak**";
                    break;
                case GuildPermission.MuteMembers:
                    result = "**Require Voice Mute**";
                    break;
                case GuildPermission.DeafenMembers:
                    result = "**Require Voice Deafen**";
                    break;
                case GuildPermission.MoveMembers:
                    result = "**Require Voice Move**";
                    break;
                case GuildPermission.UseVAD:
                    result = "";
                    break;
                case GuildPermission.PrioritySpeaker:
                    result = "";
                    break;
                case GuildPermission.ChangeNickname:
                    result = "**Require Change Nicknames**";
                    break;
                case GuildPermission.ManageNicknames:
                    result = "**Require Manage Nicknames**";
                    break;
                case GuildPermission.ManageRoles:
                    result = "**Require Manage Roles**";
                    break;
                case GuildPermission.ManageWebhooks:
                    result = "**Require ManageWebHooks**";
                    break;
                case GuildPermission.ManageEmojis:
                    result = "**Require Manage Emojis**";
                    break;
            }

            return result;
        }

        private string ChannelPermissionString(ChannelPermission perm)
        {
            string result = null;
            switch (perm)
            {
                case ChannelPermission.CreateInstantInvite:
                    result = "**Require Create Invite**";
                    break;
                case ChannelPermission.ManageChannels:
                    result = "**Require Manage Channels**";
                    break;
                case ChannelPermission.AddReactions:
                    result = "**Require Add Reactions**";
                    break;
                case ChannelPermission.ViewChannel:
                    result = "**Require View Messages**";
                    break;
                case ChannelPermission.SendMessages:
                    result = "**Require Send Messages**";
                    break;
                case ChannelPermission.SendTTSMessages:
                    result = "**Require Send TTS Messages**";
                    break;
                case ChannelPermission.ManageMessages:
                    result = "**Require Manage Messages**";
                    break;
                case ChannelPermission.EmbedLinks:
                    result = "**Require Embed Links**";
                    break;
                case ChannelPermission.AttachFiles:
                    result = "**Require Attach Files**";
                    break;
                case ChannelPermission.ReadMessageHistory:
                    result = "**Require Read Message History**";
                    break;
                case ChannelPermission.MentionEveryone:
                    result = "**Require Mention Everyone**";
                    break;
                case ChannelPermission.UseExternalEmojis:
                    result = "**Require Use External Emojis**";
                    break;
                case ChannelPermission.Connect:
                    result = "**Require Voice Connect**";
                    break;
                case ChannelPermission.Speak:
                    result = "**Require Voice Speak**";
                    break;
                case ChannelPermission.MuteMembers:
                    result = "**Require Voice Mute**";
                    break;
                case ChannelPermission.DeafenMembers:
                    result = "**Require Voice Deafen**";
                    break;
                case ChannelPermission.MoveMembers:
                    result = "**Require Voice Move**";
                    break;
                case ChannelPermission.UseVAD:
                    break;
                case ChannelPermission.PrioritySpeaker:
                    result = "**Require Priority Speaker**";
                    break;
                case ChannelPermission.ManageRoles:
                    result = "**Require Manage Roles**";
                    break;
                case ChannelPermission.ManageWebhooks:
                    result = "**Require Manage Webhook**";
                    break;
            }

            return result;
        }
    }
}