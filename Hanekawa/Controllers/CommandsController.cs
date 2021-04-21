using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Disqord.Bot;
using Disqord.Gateway;
using Hanekawa.Bot.Commands.Preconditions;
using Microsoft.AspNetCore.Mvc;
using Qmmands;
using Command = Hanekawa.Models.Command;
using Module = Hanekawa.Models.Module;

namespace Hanekawa.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommandsController : ControllerBase
    {
        private readonly Bot.Hanekawa _bot;
        public CommandsController(Bot.Hanekawa bot) => _bot = bot;

        [HttpGet]
        public List<Module> GetCommands(CancellationToken token)
        {
            var toReturn = new List<Module>();
            var modules = _bot.Commands.GetAllModules().OrderBy(x => x.Name);
            foreach (var x in modules)
            {
                if(x.Name == "Owner") continue;
                var commands = new Dictionary<string, Command>();
                foreach (var c in x.Commands)
                {
                    if (commands.TryGetValue(c.Name, out var command))
                    {
                        command.Example.Add($"{c.FullAliases.FirstOrDefault()} {ExampleParamBuilder(c)}");
                    }
                    else
                    {
                        commands.TryAdd(c.Name, new Command
                        {
                            Name = c.Name,
                            Commands = c.FullAliases.ToList(),
                            Description = c.Description,
                            Example = new List<string>
                                {$"{c.FullAliases.FirstOrDefault()} {ExampleParamBuilder(c)}"},
                            Premium = PremiumCheck(c),
                            Permissions = PermBuilder(c)
                        });
                    }
                }
                toReturn.Add(new Module
                {
                    Name = x.Name,
                    Description = x.Description,
                    Commands = commands.Select(z => z.Value).ToList()
                });
            }

            return toReturn;
        }

        private static bool PremiumCheck(Qmmands.Command cmd)
        {
            var premium = cmd.Checks.FirstOrDefault(x => x is RequirePremium) 
                          ?? cmd.Module.Checks.FirstOrDefault(x => x is RequirePremium);
            return premium != null;
        }

        private static List<string> PermBuilder(Qmmands.Command cmd)
        {
            var str = new List<string>();
            foreach (var x in cmd.Module.Checks)
            {
                if (x is RequireAuthorGuildPermissionsAttribute perm)
                {
                    str.Add(perm.Permissions.ToString());
                }
            }
            foreach (var x in cmd.Checks)
            {
                if (x is RequireAuthorGuildPermissionsAttribute perm)
                { 
                    str.Add(perm.Permissions.ToString());
                }
            }

            return str;
        }

        private string ExampleParamBuilder(Qmmands.Command command)
        {
            var output = new StringBuilder();
            if (!command.Parameters.Any()) return output.ToString();
            foreach (var x in command.Parameters)
            {
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

        private static string PermTypeBuilder(Parameter parameter) =>
            parameter.Type == typeof(CachedMember) ? "@bob#0000" :
            parameter.Type == typeof(CachedRole) ? "role" :
            parameter.Type == typeof(CachedTextChannel) ? "#General" :
            parameter.Type == typeof(CachedVoiceChannel) ? "VoiceChannel" :
            parameter.Type == typeof(CachedCategoryChannel) ? "Category" :
            parameter.Type == typeof(TimeSpan?) ? "1h2m" :
            parameter.Type == typeof(TimeSpan) ? "1h2m" :
            parameter.Type == typeof(int) ? "5" :
            parameter.Type == typeof(string) ? "Example text" :
            parameter.Type == typeof(ulong) ? "431610594290827267" : parameter.Name;
    }
}