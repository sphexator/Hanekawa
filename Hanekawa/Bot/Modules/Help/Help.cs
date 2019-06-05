using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Bot.Services.Command;
using Hanekawa.Core.Interactive;
using Hanekawa.Extensions.Embed;
using Qmmands;
using Quartz.Util;
using Cooldown = Hanekawa.Core.Cooldown;

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
            string result = null;
            var modules = _command.GetAllModules();
            for (var i = 0; i < modules.Count;)
            {
                var strBuilder = new StringBuilder();
                for (var j = 0; j < 5; j++)
                {
                    if(i >= modules.Count) continue;
                    var x = modules[i];
                    strBuilder.Append(j < 4 ? $"`{x.Name}` - " : $"`{x.Name}`");
                    i++;
                }

                result += $"{strBuilder}\n";
            }

            var embed = new EmbedBuilder().CreateDefault(result, Context.Guild.Id);
            embed.Author = new EmbedAuthorBuilder { Name = "Module list" };
            embed.Footer = new EmbedFooterBuilder { Text = $"Use `{_commandHandling.GetPrefix(Context.Guild.Id).FirstOrDefault()}help <module>` to get help with a module"};
            await Context.ReplyAsync(result);
        }

        [Name("Help")]
        [Command("help")]
        [Description("List all commands for provided module, if valid one provided")]
        [RequiredChannel]
        [Cooldown(1, 2, CooldownMeasure.Seconds, Cooldown.Whatever)]
        public async Task HelpAsync([Remainder] string module)
        {
            var result = new List<string>();
            var moduleInfo = _command.GetAllModules().FirstOrDefault(x => x.Name == module);
            if (moduleInfo == null)
            {
                await Context.ReplyAsync("Couldn't find a module with that name", Color.Red.RawValue);
                return;
            } 
            for (var i = 0; i < moduleInfo.Commands.Count; i++)
            {
                var cmd = moduleInfo.Commands[i];
                var command = cmd.Aliases.FirstOrDefault();
                var prefix = _commandHandling.GetPrefix(Context.Guild.Id).FirstOrDefault();
                var content = new StringBuilder();
                if (!cmd.Name.IsNullOrWhiteSpace()) content.AppendLine($"");
                if (!cmd.Description.IsNullOrWhiteSpace()) content.AppendLine($"");
                content.AppendLine($"{prefix}{command} {ParamBuilder(cmd)}");
                content.AppendLine($"Example: {prefix}{command} {ParamBuilder(cmd, true)}");
            }

            if (result.Count > 0)
                await PagedReplyAsync(result.PaginateBuilder(Context.Guild, "Command List", null, 10));
            else await Context.ReplyAsync("Couldn't find any commands in that module");
        }

        private string ParamBuilder(Command command, bool example = false)
        {
            var output = new StringBuilder();
            if (!command.Parameters.Any()) return output.ToString();
            for (var i = 0; i < command.Parameters.Count; i++)
            {
                var x = command.Parameters[i];
                var name = example ? PermTypeBuilder(x) : x.Name;
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

        private string PermTypeBuilder(Parameter parameter)
        {
            if (parameter.Attributes is SocketGuildUser) return "@bob#0000";
            else if (parameter.Type is SocketRole) return "role";
            else if (parameter.Type is SocketTextChannel) return "#General";
            else if (parameter.Type is SocketCategoryChannel) return "General";
            else if (parameter.Type is int) return "5";
            else return parameter.Name;
        }
    }
}