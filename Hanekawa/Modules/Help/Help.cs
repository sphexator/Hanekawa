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
                var content = string.Join(", ", await GetModulesAsync(_commands, Context));
                var embed = new EmbedBuilder()
                    .CreateDefault(content, Context.Guild.Id)
                    .WithAuthor(new EmbedAuthorBuilder {Name = "Module list"})
                    .WithFooter(new EmbedFooterBuilder {Text = $"Use `{prefix}help <module>` to get help with a module"});
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
        [Ratelimit(1, 2, Measure.Seconds)]
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

        private string GetModules()
        {
            string result = null;
            var modules = _commands.Modules.ToList();
            for (var i = 0; i < _commands.Modules.Count();)
            {
                var stringBuilder = new StringBuilder();
                for (var j = 0; j < 5; j++)
                {
                    var x = modules[i];
                    stringBuilder.Append(j == 4 ? $"`{x.Name}`" : $"`{x.Name}` - ");
                    i++;
                }

                result += $"{stringBuilder}\n";
            }

            return result;
        }
    }
}