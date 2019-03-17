using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Extensions.Embed;
using Hanekawa.Preconditions;
using Hanekawa.Services.CommandHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hanekawa.Modules.Help
{
    public class Help : InteractiveBase
    {
        private readonly CommandService _commands;
        private readonly IServiceProvider _map;
        private readonly CommandHandlingService _commandHandling;

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
                    .WithFooter(new EmbedFooterBuilder {Text = $"Use `{prefix}help <module>` to get help with a module"});
                await Context.ReplyAsync(embed);
            }
            else
            {
                var mod = _commands.Modules.FirstOrDefault(
                    m => string.Equals(m.Name.Replace("Module", "").ToLower(), path.ToLower(), StringComparison.CurrentCultureIgnoreCase));
                if (mod == null)
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
                    if(i >= modules.Count) continue;
                    var x = modules[i];

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
                result.Add($"**{x.Name}**\n" +
                           $"{x.Summary}\n" +
                           $"Usage: **{prefix}{GetPrefix(x)} {ParamBuilder(x)}**\n" +
                           $"Example: **{x.Remarks}**\n");
            }

            return result;
        }

        private string ParamBuilder(CommandInfo command)
        {
            var output = new StringBuilder();
            if (!command.Parameters.Any()) return output.ToString();
            foreach (var x in command.Parameters)
            {
                if (x.IsOptional)
                {
                    output.Append($"[{x.Name} = {x.DefaultValue}] ");
                }
                else if (x.IsRemainder)
                {
                    output.Append($"...{x.Name} ");
                }
                else if (x.IsMultiple)
                {
                    output.Append($"|{x.Name}| ");
                }
                else
                {
                    output.Append($"<{x.Name}> ");
                }
            }

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
    }
}