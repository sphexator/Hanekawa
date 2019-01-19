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
        [Summary("Lists this bots commands.")]
        [Ratelimit(1, 5, Measure.Seconds)]
        [Priority(1)]
        [RequiredChannel]
        public async Task HelpAsync([Remainder] string path = "")
        {
            if (path == "")
            {
                var content = string.Join(", ", await GetModulesAsync(_commands, Context));
                var embed = new EmbedBuilder()
                    .CreateDefault(content, Context.Guild.Id)
                    .WithAuthor(new EmbedAuthorBuilder {Name = "Module list"})
                    .WithFooter(new EmbedFooterBuilder {Text = "Use `h.help <module>` to get help with a module"});
                await Context.ReplyAsync(embed);
            }
            else
            {
                var output = new EmbedBuilder().CreateDefault(Context.Guild.Id);
                var mod = _commands.Modules.FirstOrDefault(
                    m => string.Equals(m.Name.Replace("Module", ""), path, StringComparison.CurrentCultureIgnoreCase));
                if (mod == null)
                {
                    await Context.ReplyAsync("No module could be found with that name.", Color.Red.RawValue);
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

                await ReplyAsync(null, false, output.Build());
            }
        }

        [Command("help")]
        [Summary("Lists this bots commands.")]
        [Ratelimit(1, 5, Measure.Seconds)]
        public async Task DmHelpAsync([Remainder] string path = "")
        {
            if (path == "")
            {
                var content = string.Join(", ", await GetModulesAsync(_commands, Context));
                var embed = new EmbedBuilder()
                    .CreateDefault(content, Context.Guild.Id)
                    .WithAuthor(new EmbedAuthorBuilder {Name = "Module list"})
                    .WithFooter(new EmbedFooterBuilder {Text = "Use `h.help <module>` to get help with a module"});
                ;
                embed.AddField("Support", "[Discord](https://discord.gg/gGu5TT6)", true);
                embed.AddField("Bot Invite",
                    "[link](https://discordapp.com/api/oauth2/authorize?client_id=431610594290827267&scope=bot&permissions=8)",
                    true);
                var eng = await Context.User.GetOrCreateDMChannelAsync();
                await eng.ReplyAsync(embed);
            }
            else
            {
                var output = new EmbedBuilder().CreateDefault(Context.Guild.Id);
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

                await (await Context.User.GetOrCreateDMChannelAsync()).SendMessageAsync(null, false, output.Build());
            }
        }

        private static async Task<IEnumerable<string>> GetModulesAsync(CommandService commandService,
            ICommandContext context)
        {
            var modules = new List<string>();
            foreach (var module in commandService.Modules)
            {
                var resultingList = new List<string>();
                foreach (var cmd in module.Commands)
                {
                    var result = await cmd.CheckPreconditionsAsync(context);
                    if (result.IsSuccess) resultingList.Add(cmd.Name);
                }

                if (resultingList.Count != 0) modules.Add($"`{module.Name}`");
            }

            return modules;
        }

        private void AddCommands(ModuleInfo module, ref EmbedBuilder builder)
        {
            foreach (var command in module.Commands)
            {
                var check = command.CheckPreconditionsAsync(Context, _map).GetAwaiter().GetResult();
                if (check.IsSuccess) AddCommand(command, ref builder);
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