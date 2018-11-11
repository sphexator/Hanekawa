using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Addons.Patreon;
using Hanekawa.Modules.Account.Profile;

namespace Hanekawa.Modules.Development
{
    public class Test : InteractiveBase
    {
        private readonly ProfileGenerator _generator;

        public Test(ProfileGenerator generator)
        {
            _generator = generator;
        }

        [Command("roleid", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task GetRoleID([Remainder] string roleName)
        {
            var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == roleName);
            await ReplyAsync($"{role.Name}\n" +
                             $"{role.Id}\n" +
                             $"{role.Color.RawValue}\n" +
                             $"{role.Position}");
        }

        [Command("test", RunMode = RunMode.Async)]
        [RequireOwner]
        [RequireContext(ContextType.Guild)]
        public async Task TestRules(int size)
        {
            using (var profile = await _generator.Create(Context.User as SocketGuildUser))
            {
                profile.Seek(0, SeekOrigin.Begin);
                await Context.Channel.SendFileAsync(profile, "banner.png");
            }
        }

        private string ParseEmoteString(Emote emote)
        {
            return emote.Animated ? $"<a:{emote.Name}:{emote.Id}>" : $"<{emote.Name}:{emote.Id}>";
        }

        [Command("patreon")]
        [RequireOwner]
        public async Task PatreonTest()
        {
            var patreon = new PatreonClient();
            await patreon.InitializeAsync();
            var result = await patreon.GetPledges();

            foreach (var x in result)
            {
                Console.WriteLine($"{x.Users.DiscordId} - {x.Pledges.AmountCents}");
            }
        }
    }
}