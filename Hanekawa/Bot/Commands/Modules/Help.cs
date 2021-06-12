using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Disqord;
using Disqord.Bot;
using Hanekawa.Bot.Commands.Preconditions;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Entities.Color;
using Hanekawa.Extensions;
using Qmmands;

namespace Hanekawa.Bot.Commands.Modules
{
    [Name("Help")]
    [Description("Displays all commands and how to execute them")]
    [RequiredChannel]
    public class Help : HanekawaCommandModule
    {
        private readonly CacheService _cache;
        public Help(CacheService cache) => _cache = cache;

        [Name("Help")]
        [Command("help")]
        [Description("List all modules")]
        [Priority(1)]
        [Cooldown(1, 2, CooldownMeasure.Seconds, CooldownBucketType.Member)]
        public DiscordCommandResult HelpAsync()
        {
            var result = new StringBuilder();
            var modules = Context.Bot.Commands.GetAllModules();
            //var settingModules = modules.Where(x => x.Checks.Contains<>(typeof(RequirePremium))).ToList();
            for (var i = 0; i < modules.Count;)
            {
                var str = new StringBuilder();
                for (var j = 0; j < 5; j++)
                {
                    if (i >= modules.Count) continue;
                    var x = modules[i];
                    if (x.Name == "Owner" || x.Parent != null)
                    {
                        i++;
                        continue;
                    }
                    str.Append((j < 4 || i < modules.Count - 1) ? $"`{x.Name}` - " : $"`{x.Name}`");
                    i++;
                }

                result.AppendLine($"{str}");
            }

            return Reply(new LocalEmbed
            {
                Color = _cache.GetColor(Context.GuildId),
                Author = new LocalEmbedAuthor {Name = "Module list"},
                Description = result.ToString(),
                Footer = new LocalEmbedFooter
                {
                    Text =
                        $"Use `{Context.Prefix}help <module>` to get help with a module"
                }
            });
        }

        [Name("Help")]
        [Command("help")]
        [Description("List all commands for provided module, if valid one provided")]
        [Cooldown(1, 2, CooldownMeasure.Seconds, CooldownBucketType.Member)]
        public DiscordCommandResult HelpAsync([Remainder] string module)
        {
            var moduleInfo = Context.Bot.Commands.GetAllModules().FirstOrDefault(x =>
                string.Equals(x.Name, module, StringComparison.CurrentCultureIgnoreCase));
            if (moduleInfo == null)
            {
                var response = new StringBuilder();
                var moduleList = new List<Tuple<Module, int>>();
                var modules = Context.Bot.Commands.GetAllModules();
                for (var i = 0; i < modules.Count; i++)
                {
                    if (i >= modules.Count) continue;
                    var x = modules[i];
                    if (x.Name.FuzzyMatch(module, out var score)) moduleList.Add(new Tuple<Module, int>(x, score));
                }
                
                var orderedList = moduleList.OrderByDescending(x => x.Item2).ToList();
                switch (orderedList.Count)
                {
                    case 0:
                        response.AppendLine("No module matches that search");
                        break;
                    case 1:
                        moduleInfo = orderedList[0].Item1;
                        break;
                    default:
                    {
                        response.AppendLine("Found multiple matches, did you mean:");
                        var amount = moduleList.Count > 5 ? 5 : moduleList.Count;
                        for (var i = 0; i < amount; i++)
                        {
                            var x = orderedList[i];
                            response.AppendLine($"{i + 1}: **{x.Item1.Name}**");
                        }

                        break;
                    }
                }
                
                if (moduleInfo == null)
                {
                    return Reply(new LocalEmbed
                    {
                        Color = _cache.GetColor(Context.GuildId),
                        Author = new LocalEmbedAuthor {Name = "Module list"},
                        Title = "Couldn't find a module with that name",
                        Description = response.ToString(),
                        Footer = new LocalEmbedFooter
                            {Text = $"Use `{Context.Prefix}help <module>` to get help with a module"}
                    });
                }
            }
            if (moduleInfo.Name == "Owner") return null;
            var result = new List<string>();
            foreach (var cmd in moduleInfo.Commands)
                result.FormatCommandText(cmd, Context.Prefix);

            foreach (var xModule in moduleInfo.Submodules)
            foreach (var x in xModule.Commands)
                result.FormatCommandText(x, Context.Prefix);

            if (result.Count > 0)
                return Pages(result.Pagination(_cache.GetColor(Context.GuildId), Context.Guild.GetIconUrl(),
                    "Command List"));
            return Reply("Couldn't find any commands in that module", HanaBaseColor.Bad());
        }
    }
}