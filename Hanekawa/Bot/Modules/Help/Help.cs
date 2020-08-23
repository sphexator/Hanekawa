﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Command.Extensions;
using Qmmands;
using Quartz.Util;

namespace Hanekawa.Bot.Modules.Help
{
    [Name("Help")]
    [Description("Displays all commands and how to execute them")]
    public class Help : HanekawaCommandModule
    {
        [Name("Help")]
        [Command("help")]
        [Description("List all modules")]
        [Priority(1)]
        [RequiredChannel]
        [Cooldown(1, 2, CooldownMeasure.Seconds, HanaCooldown.Whatever)]
        public async Task HelpAsync()
        {
            var result = new StringBuilder();
            var modules = Context.Bot.GetAllModules();
            for (var i = 0; i < modules.Count;)
            {
                var strBuilder = new StringBuilder();
                for (var j = 0; j < 5; j++)
                {
                    if (i >= modules.Count) continue;
                    var x = modules[i];
                    if (x.Name == "Owner")
                    {
                        i++;
                        continue;
                    }
                    strBuilder.Append(j < 4 ? $"`{x.Name}` - " : $"`{x.Name}`");
                    i++;
                }

                result.AppendLine($"{strBuilder}");
            }

            var embed = new LocalEmbedBuilder().Create(result.ToString(), Context.Colour.Get(Context.Guild.Id.RawValue));
            embed.Author = new LocalEmbedAuthorBuilder {Name = "Module list"};
            embed.Footer = new LocalEmbedFooterBuilder
            {
                Text =
                    $"Use `{Context.Prefix}help <module>` to get help with a module"
            };
            await Context.ReplyAsync(embed);
        }

        [Name("Help")]
        [Command("help")]
        [Description("List all commands for provided module, if valid one provided")]
        [RequiredChannel]
        [Cooldown(1, 2, CooldownMeasure.Seconds, HanaCooldown.Whatever)]
        public async Task HelpAsync([Remainder] string module)
        {
            var moduleInfo = Context.Bot.GetAllModules().FirstOrDefault(x =>
                string.Equals(x.Name, module, StringComparison.CurrentCultureIgnoreCase));
            if (moduleInfo == null)
            {
                var response = new StringBuilder();
                var moduleList = new List<Tuple<Module, int>>();
                var modules = Context.Bot.GetAllModules();
                for (var i = 0; i < modules.Count; i++)
                {
                    if (i >= modules.Count) continue;
                    var x = modules[i];
                    if (x.Name.FuzzyMatch(module, out var score)) moduleList.Add(new Tuple<Module, int>(x, score));
                }

                var orderedList = moduleList.OrderByDescending(x => x.Item2).ToList();
                if (orderedList.Count == 0) response.AppendLine("No module matches that search");
                if (orderedList.Count == 1) moduleInfo = orderedList.First().Item1;
                
                else
                {
                    response.AppendLine("Found multiple matches, did you mean:");
                    var amount = moduleList.Count > 5 ? 5 : moduleList.Count;
                    for (var i = 0; i < amount; i++)
                    {
                        var x = orderedList[i];
                        response.AppendLine($"{i + 1}: **{x.Item1.Name}**");
                    }
                }

                if (moduleInfo == null)
                {
                    var embed = new LocalEmbedBuilder().Create(response.ToString(), Context.Colour.Get(Context.Guild.Id.RawValue));
                    embed.Author = new LocalEmbedAuthorBuilder { Name = "Module list" };
                    embed.Title = "Couldn't find a module with that name";
                    embed.Footer = new LocalEmbedFooterBuilder
                    {
                        Text =
                            $"Use `{Context.Prefix}help <module>` to get help with a module"
                    };
                    await Context.ReplyAsync(embed);
                    return;
                }
            }
            if (moduleInfo.Name == "Owner") return;
            var result = new List<string>();
            for (var i = 0; i < moduleInfo.Commands.Count; i++)
            {
                var cmd = moduleInfo.Commands[i];
                var command = cmd.Aliases.FirstOrDefault();
                var content = new StringBuilder();
                var perms = PermBuilder(cmd);
                content.AppendLine(!cmd.Name.IsNullOrWhiteSpace()
                    ? $"**{cmd.Name}**"
                    : $"**{cmd.Aliases.FirstOrDefault()}**");
                if (!perms.IsNullOrWhiteSpace()) content.Append($"**Require {perms}** ");
                if (PremiumCheck(cmd, out var prem)) content.AppendLine(prem);
                content.AppendLine(
                    $"Alias: **{cmd.Aliases.Aggregate("", (current, cmdName) => current + $"{cmdName}, ")}**");
                if (!cmd.Description.IsNullOrWhiteSpace()) content.AppendLine(cmd.Description);
                if (!cmd.Remarks.IsNullOrWhiteSpace()) content.AppendLine(cmd.Remarks);
                content.AppendLine($"Usage: **{Context.Prefix}{command} {ParamBuilder(cmd)}**");
                content.AppendLine($"Example: {Context.Prefix}{command} {ExampleParamBuilder(cmd)}");
                result.Add(content.ToString());
            }

            if (result.Count > 0)
                await Context.PaginatedReply(result, Context.Guild, "Command List");
            else await Context.ReplyAsync("Couldn't find any commands in that module", Color.Red);
        }

        // Showcases the params
        private string ParamBuilder(Command command)
        {
            var output = new StringBuilder();
            if (!command.Parameters.Any()) return output.ToString();
            for (var i = 0; i < command.Parameters.Count; i++)
            {
                var x = command.Parameters[i];
                var name = x.Name;
                if (x.IsOptional)
                    output.Append($"[{name} = {x.DefaultValue}] ");
                else if (x.IsRemainder)
                    output.Append($"...{name} ");
                else if (x.IsMultiple)
                    output.Append($"|{name} etc...|");
                else
                    output.Append($"<{name}> ");
            }

            return output.ToString();
        }

        // Adds examples, ie. for user it adds bob#0000
        private string ExampleParamBuilder(Command command)
        {
            var output = new StringBuilder();
            if (!command.Parameters.Any()) return output.ToString();
            for (var i = 0; i < command.Parameters.Count; i++)
            {
                var x = command.Parameters[i];
                var name = PermTypeBuilder(x);
                if (x.IsOptional)
                    output.Append($"{name} ");
                else if (x.IsRemainder)
                    output.Append($"{name} ");
                else if (x.IsMultiple)
                    output.Append($"{name} ");
                else
                    output.Append($"{name} ");
            }

            return output.ToString();
        }

        private string PermTypeBuilder(Parameter parameter) =>
            parameter.Type == typeof(CachedMember) ? "@bob#0000" :
            parameter.Type == typeof(CachedRole) ? "role" :
            parameter.Type == typeof(CachedTextChannel) ? "#General" :
            parameter.Type == typeof(CachedVoiceChannel) ? "VoiceChannel" :
            parameter.Type == typeof(CachedCategoryChannel) ? "Category" :
            parameter.Type == typeof(int) ? "5" :
            parameter.Type == typeof(string) ? "Example text" :
            parameter.Type == typeof(ulong) ? "431610594290827267" : parameter.Name;

        // Appends permission requirements
        private string PermBuilder(Command cmd)
        {
            var str = new StringBuilder();
            foreach (var x in cmd.Module.Checks)
            {
                if (x is RequireMemberGuildPermissionsAttribute perm)
                {
                    if (perm.Permissions.Count() == 1) str.AppendLine(perm.Permissions.FirstOrDefault().ToString());
                    else foreach (var e in perm.Permissions) str.Append($"{e}, ");
                }
            }
            foreach (var x in cmd.Checks)
            {
                if (x is RequireMemberGuildPermissionsAttribute perm)
                {
                    if (perm.Permissions.Count() == 1) str.AppendLine(perm.Permissions.FirstOrDefault().ToString());
                    else foreach (var e in perm.Permissions) str.Append($"{e}, ");
                }
            }

            return str.ToString();
        }

        private bool PremiumCheck(Command cmd, out string perm)
        {
            var premium = cmd.Checks.FirstOrDefault(x => x is RequirePremium);
            if (premium == null) premium = cmd.Module.Checks.FirstOrDefault(x => x is RequirePremium);
            if (premium != null)
            {
                perm = "**Require Premium**";
                return true;
            }

            perm = null;
            return false;
        }
    }
}