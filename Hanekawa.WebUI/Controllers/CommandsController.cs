using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Hanekawa.WebUI.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Hanekawa.WebUI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommandsController : ControllerBase
    {
        private readonly Bot.Hanekawa _bot;
        public CommandsController(Bot.Hanekawa bot) => _bot = bot;

        [HttpGet]
        public List<Models.Module> GetCommands(CancellationToken token)
        {
            var toReturn = new List<Models.Module>();
            var modules = _bot.Commands.GetAllModules().Where(x => x.Name != "Owner").OrderBy(x => x.Name).ToList();
            foreach (var x in modules)
            {
                if(x.Name == "Owner") continue;
                var commands = new Dictionary<string, Models.Command>();
                foreach (var c in x.Commands)
                {
                    if (commands.TryGetValue(c.Name, out var command))
                        command.Example.Add($"{c.FullAliases.FirstOrDefault()} {c.ExampleParam()}");
                    else
                    {
                        commands.TryAdd(c.Name, new Models.Command
                        {
                            Name = c.Name,
                            Commands = c.FullAliases.ToList(),
                            Description = c.Description,
                            Example = new List<string>
                                { $"{c.FullAliases.FirstOrDefault()} {c.ExampleParam()}" },
                            Premium = c.PremiumCheck(out _),
                            Permissions = c.Perm()
                        });
                    }
                }
                toReturn.Add(new Models.Module
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