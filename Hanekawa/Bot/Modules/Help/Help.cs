using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Bot.Services.Command;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Interactive;
using Qmmands;
using Quartz.Util;
using Cooldown = Hanekawa.Shared.Command.Cooldown;

namespace Hanekawa.Bot.Modules.Help
{
    [Name("Help")]
    [Description("Displays all commands and how to execute them")]
    public class Help : InteractiveBase
    {
        private readonly CommandService _command;
        private readonly CommandHandlingService _commandHandling;

        public Help(CommandService command, CommandHandlingService commandHandling)
        {
            _command = command;
            _commandHandling = commandHandling;
        }

        [Name("Help")]
        [Command("help")]
        [Description("List all modules")]
        [Priority(1)]
        [RequiredChannel]
        [Cooldown(1, 2, CooldownMeasure.Seconds, Cooldown.Whatever)]
        public async Task HelpAsync()
        {
            var result = new StringBuilder();
            var modules = _command.GetAllModules();
            for (var i = 0; i < modules.Count;)
            {
                var strBuilder = new StringBuilder();
                for (var j = 0; j < 5; j++)
                {
                    if (i >= modules.Count) continue;
                    var x = modules[i];
                    strBuilder.Append(j < 4 ? $"`{x.Name}` - " : $"`{x.Name}`");
                    i++;
                }

                result.AppendLine($"{strBuilder}");
            }

            var embed = new EmbedBuilder().Create(result.ToString(), Context.Colour.Get(Context.Guild.Id));
            embed.Author = new EmbedAuthorBuilder {Name = "Module list"};
            embed.Footer = new EmbedFooterBuilder
            {
                Text =
                    $"Use `{_commandHandling.GetPrefix(Context.Guild.Id)}help <module>` to get help with a module"
            };
            await Context.ReplyAsync(embed);
        }

        [Name("Help")]
        [Command("help")]
        [Description("List all commands for provided module, if valid one provided")]
        [RequiredChannel]
        [Cooldown(1, 2, CooldownMeasure.Seconds, Cooldown.Whatever)]
        public async Task HelpAsync([Remainder] string module)
        {
            var moduleInfo = _command.GetAllModules().FirstOrDefault(x =>
                string.Equals(x.Name, module, StringComparison.CurrentCultureIgnoreCase));
            if (moduleInfo == null)
            {
                var response = new StringBuilder();
                var moduleList = new List<Tuple<Module, int>>();
                var modules = _command.GetAllModules();
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
                        response.AppendLine($"{i + 1}: {Format.Bold(x.Item1.Name)}");
                    }
                }

                if (moduleInfo == null)
                {
                    var embed = new EmbedBuilder().Create(response.ToString(), Context.Colour.Get(Context.Guild.Id));
                    embed.Author = new EmbedAuthorBuilder { Name = "Module list" };
                    embed.Title = "Couldn't find a module with that name";
                    embed.Footer = new EmbedFooterBuilder
                    {
                        Text =
                            $"Use `{_commandHandling.GetPrefix(Context.Guild.Id)}help <module>` to get help with a module"
                    };
                    await Context.ReplyAsync(embed);
                    return;
                }
            }

            var result = new List<string>();
            for (var i = 0; i < moduleInfo.Commands.Count; i++)
            {
                var cmd = moduleInfo.Commands[i];
                var command = cmd.Aliases.FirstOrDefault();
                var prefix = _commandHandling.GetPrefix(Context.Guild.Id);
                var content = new StringBuilder();
                var perms = PermBuilder(cmd);
                content.AppendLine(!cmd.Name.IsNullOrWhiteSpace()
                    ? Format.Bold(cmd.Name)
                    : Format.Bold(cmd.Aliases.FirstOrDefault()));
                if (!perms.IsNullOrWhiteSpace()) content.AppendLine(Format.Bold($"Require {perms}"));
                content.AppendLine(
                    $"Alias: {Format.Bold(cmd.Aliases.Aggregate("", (current, cmdName) => current + $"{cmdName}, "))}");
                if (!cmd.Description.IsNullOrWhiteSpace()) content.AppendLine(cmd.Description);
                if (!cmd.Remarks.IsNullOrWhiteSpace()) content.AppendLine(cmd.Remarks);
                content.AppendLine($"Usage: {Format.Bold($"{prefix}{command} {ParamBuilder(cmd)}")}");
                content.AppendLine($"Example: {Format.Bold($"{prefix}{command} {ExampleParamBuilder(cmd)}")}");
                result.Add(content.ToString());
            }

            if (result.Count > 0)
                await Context.ReplyPaginated(result, Context.Guild, "Command List");
            else await Context.ReplyAsync("Couldn't find any commands in that module", Color.Red);
        }

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

        private string PermTypeBuilder(Parameter parameter)
        {
            if (parameter.Type == typeof(SocketGuildUser)) return "@bob#0000";
            if (parameter.Type == typeof(SocketRole)) return "role";
            if (parameter.Type == typeof(SocketTextChannel)) return "#General";
            if (parameter.Type == typeof(SocketVoiceChannel)) return "VoiceChannel";
            if (parameter.Type == typeof(SocketCategoryChannel)) return "Category";
            if (parameter.Type == typeof(int)) return "5";
            if (parameter.Type == typeof(string)) return "Example text";
            if (parameter.Type == typeof(ulong)) return "431610594290827267";
            return parameter.Name;
        }

        private string PermBuilder(Command cmd)
        {
            var str = new StringBuilder();
            foreach (var x in cmd.Checks)
            {
                if (x is RequireUserPermission perm)
                {
                    if (perm.Perms.Length == 1) str.AppendLine(perm.Perms.FirstOrDefault().ToString());
                    else foreach (var e in perm.Perms) str.Append($"{e.ToString()}, ");
                }
            }

            return str.ToString();
        }
    }
}