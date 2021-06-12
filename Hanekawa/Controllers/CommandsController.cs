using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Hanekawa.Extensions;
using Microsoft.AspNetCore.Mvc;
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
                        command.Example.Add($"{c.FullAliases.FirstOrDefault()} {c.ExampleParam()}");
                    }
                    else
                    {
                        commands.TryAdd(c.Name, new Command
                        {
                            Name = c.Name,
                            Commands = c.FullAliases.ToList(),
                            Description = c.Description,
                            Example = new List<string>
                                {$"{c.FullAliases.FirstOrDefault()} {c.ExampleParam()}"},
                            Premium = c.PremiumCheck(out _),
                            Permissions = c.Perm()
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
    }
}