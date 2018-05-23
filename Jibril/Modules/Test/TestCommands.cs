using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Jibril.Services;
using Jibril.Services.INC;
using Jibril.Services.Level;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jibril.Modules.Test
{
    public class TestCommands : InteractiveBase
    {
        private readonly HungerGames _hg;
        public TestCommands(HungerGames hg)
        {
            _hg = hg;
        }

        [Command("role")]
        [RequireOwner]
        public async Task GetRoleId(string name)
        {
            var role = Context.Guild.Roles.First(x => x.Name == name);
            if (role == null) return;
            await ReplyAsync($"{role.Name} - {role.Id}");
        }

        [Command("sign", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task SignTask()
        {
            try
            {
                await _hg.StartSignUp();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        [Command("start", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task StartTask()
        {
            try
            {
                await _hg.StartEvent();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }

        }

        [Command("end", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task EndGame()
        {
            _hg.EndGame();
            await ReplyAsync("Cleared database");
        }

        [Command("continue", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task ContinueTask()
        {
            try
            {
                await _hg.ContinueEvent();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [Command("test")]
        [RequireOwner]
        public async Task TestCommandTask()
        {
            var role = Context.Client.GetGuild(200265036596379648).Roles.FirstOrDefault(x => x.Name == "Kai Ni");
            var oldMvps = role?.Members;
            var ma = DatabaseService.GetActiveUsers();
            var newMvps = new List<IGuildUser>();
            foreach (var x in ma)
            {
                var user = Context.Guild.GetUser(x);
                newMvps.Add(user);
            }

            var embed = AmInfamous.MvpMessage(oldMvps, oldMvps);
            await ReplyAsync("", false, embed.Build());
        }
    }
}