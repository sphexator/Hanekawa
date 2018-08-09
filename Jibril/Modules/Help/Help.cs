using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;

namespace Hanekawa.Modules.Help
{
    public class Help : InteractiveBase
    {
        private readonly CommandService _commands;
        private readonly IServiceProvider _map;

        public Help(IServiceProvider map, CommandService commands)
        {
            _commands = commands;
            _map = map;
        }

        [Command("help")]
        [Summary("Lists this bot's commands.")]
        public async Task HelpAsync(string path = "")
        {
            if (path == "")
            {
                var content = string.Join(", ", GetModules(_commands));
                var author = new EmbedAuthorBuilder
                {
                    Name = "Module list"
                };
                var footer = new EmbedFooterBuilder
                {
                    Text = "Use `help <module>` to get help with a module"
                };
                var embed = new EmbedBuilder
                {
                    Color = Color.Purple,
                    Description = content,
                    Author = author,
                    Footer = footer
                };
                await ReplyAsync(null, false, embed.Build());
            }
            else
            {
                var output = new EmbedBuilder {Color = Color.Purple};
                var mod = _commands.Modules.FirstOrDefault(
                    m => string.Equals(m.Name.Replace("Module", ""), path, StringComparison.CurrentCultureIgnoreCase));
                if (mod == null)
                {
                    await ReplyAsync("No module could be found with that name.");
                    return;
                }

                output.Title = mod.Name;
                output.Description = $"{mod.Summary}\n" +
                                     (!string.IsNullOrEmpty(mod.Remarks) ? $"({mod.Remarks})\n" : "") +
                                     (mod.Aliases.Any() ? $"Prefix(es): {string.Join(",", mod.Aliases)}\n" : "") +
                                     (mod.Submodules.Any()
                                         ? $"Submodules: {mod.Submodules.Select(m => m.Name)}\n"
                                         : "") + " ";
                AddCommands(mod, ref output);

                await ReplyAsync("", embed: output.Build());
            }
        }

        private static IEnumerable<string> GetModules(CommandService commandService)
        {
            return commandService.Modules.Select(x => $"`{x.Name}`").ToList();
        }

        private void AddCommands(ModuleInfo module, ref EmbedBuilder builder)
        {
            foreach (var command in module.Commands)
            {
                command.CheckPreconditionsAsync(Context, _map).GetAwaiter().GetResult();
                AddCommand(command, ref builder);
            }
        }

        private static void AddCommand(CommandInfo command, ref EmbedBuilder builder)
        {
            builder.AddField(f =>
            {
                f.Name = $"**{command.Name}**";
                f.Value = $"{command.Summary}\n" +
                          (!string.IsNullOrEmpty(command.Remarks) ? $"({command.Remarks})\n" : "") +
                          (command.Aliases.Any()
                              ? $"**Aliases:** {string.Join(", ", command.Aliases.Select(x => $"`{x}`"))}\n"
                              : "") +
                          $"**Usage:** `{GetPrefix(command)} {GetAliases(command)}`";
            });
        }

        private static string GetAliases(CommandInfo command)
        {
            var output = new StringBuilder();
            if (!command.Parameters.Any()) return output.ToString();
            foreach (var param in command.Parameters)
                if (param.IsOptional)
                    output.Append($"[{param.Name} = {param.DefaultValue}] ");
                else if (param.IsMultiple)
                    output.Append($"|{param.Name}| ");
                else if (param.IsRemainder)
                    output.Append($"...{param.Name} ");
                else
                    output.Append($"<{param.Name}> ");
            return output.ToString();
        }

        private static string GetPrefix(CommandInfo command)
        {
            var output = GetPrefix(command.Module);
            output += $"{command.Aliases.FirstOrDefault()} ";
            return output;
        }

        private static string GetPrefix(ModuleInfo module)
        {
            var output = "";
            if (module.Parent != null) output = $"{GetPrefix(module.Parent)}{output}";
            //if (module.Aliases.Any()) output += string.Concat(module.Aliases.FirstOrDefault(), " ");
            return output;
        }
    }
}