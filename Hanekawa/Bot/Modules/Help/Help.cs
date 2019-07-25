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
        [RequiredChannel]
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

            var embed = new EmbedBuilder().CreateDefault(result.ToString());
            embed.Author = new EmbedAuthorBuilder {Name = "Module list"};
            embed.Footer = new EmbedFooterBuilder
            {
                Text =
                    $"Use `{_commandHandling.GetPrefix(Context.Guild.Id).FirstOrDefault()}help <module>` to get help with a module"
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
                if (orderedList.Count > 0)
                {
                    response.AppendLine("Did you mean:");
                    var amount = moduleList.Count > 5 ? 5 : moduleList.Count;
                    for (var i = 0; i < amount; i++)
                    {
                        var x = orderedList[i];
                        response.AppendLine(x.Item1.Name);
                    }
                }
                else
                {
                    response.AppendLine("No module matches that search");
                }

                var embed = new EmbedBuilder().CreateDefault(response.ToString());
                embed.Author = new EmbedAuthorBuilder {Name = "Module list"};
                embed.Title = "Couldn't find a module with that name";
                embed.Footer = new EmbedFooterBuilder
                {
                    Text =
                        $"Use `{_commandHandling.GetPrefix(Context.Guild.Id).FirstOrDefault()}help <module>` to get help with a module"
                };
                await Context.ReplyAsync(embed);
                return;
            }

            var result = new List<string>();
            for (var i = 0; i < moduleInfo.Commands.Count; i++)
            {
                var cmd = moduleInfo.Commands[i];
                var command = cmd.Aliases.FirstOrDefault();
                var prefix = _commandHandling.GetPrefix(Context.Guild.Id).FirstOrDefault();
                var content = new StringBuilder();
                if (!cmd.Name.IsNullOrWhiteSpace()) content.AppendLine($"**{cmd.Name}**");
                if (!cmd.Description.IsNullOrWhiteSpace()) content.AppendLine(cmd.Description);
                if (!cmd.Remarks.IsNullOrWhiteSpace()) content.AppendLine(cmd.Remarks);
                content.AppendLine(
                    $"Alias: {cmd.Aliases.Aggregate("", (current, cmdName) => current + $"{cmdName}, ")}");
                content.AppendLine($"{prefix}{command} {ParamBuilder(cmd)}");
                content.AppendLine($"Example: {prefix}{command} {ExampleParamBuilder(cmd)}");
                result.Add(content.ToString());
            }

            if (result.Count > 0)
                await Context.ReplyPaginated(result, Context.Guild, "Command List", null, 10);
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
                    output.Append(x.DefaultValue == null
                        ? $"{name} (optional) "
                        : $"{name} = {x.DefaultValue} (optional) ");
                else if (x.IsRemainder)
                    output.Append($"...{name} ");
                else if (x.IsMultiple)
                    output.Append($"{name} etc...");
                else
                    output.Append($"{name} ");
            }

            return output.ToString();
        }

        private string PermTypeBuilder(Parameter parameter)
        {
            if (parameter.Type is SocketGuildUser) return "@bob#0000";
            if (parameter.Type is SocketRole) return "role";
            if (parameter.Type is SocketTextChannel) return "#General";
            if (parameter.Type is SocketCategoryChannel) return "General";
            return parameter.Name;
        }
    }
}