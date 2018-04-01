using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Jibril.Services.INC;

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

        [Command("continue", RunMode = RunMode.Async)]
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
            var msg = await ReplyAsync("Test message");
            Emote.TryParse("<:yes:402675768334876674>", out var yesEmote);
            Emote.TryParse("<:no:402675767814914049>", out var noEmote);
            IEmote iemoteYes = yesEmote;
            IEmote iemoteNo = noEmote;
            await msg.AddReactionAsync(iemoteYes);
            await msg.AddReactionAsync(iemoteNo);
        }
    }
}