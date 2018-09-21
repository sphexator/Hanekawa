using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;

namespace Hanekawa.Modules.Development
{
    public class Test : InteractiveBase
    {
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
        public async Task TestRules(Emote emote)
        {
            var result = ParseEmoteString(emote);
            await ReplyAsync(result);
            Emote.TryParse(result, out var resultEmote);
            await ReplyAsync($"{resultEmote}");
        }

        private string ParseEmoteString(Emote emote)
        {
            return emote.Animated ? $"<a:{emote.Name}:{emote.Id}>" : $"<{emote.Name}:{emote.Id}>";
        }

        [Command("users", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task GetUsers()
        {
            await Context.Guild.DownloadUsersAsync();
            Console.WriteLine(Context.Guild.Users.Count);
            foreach (var x in Context.Guild.Users)
            {
                Console.WriteLine($"{x.Username} - {x.Id}");
            }
            Console.WriteLine(Context.Guild.Users.Count);
        }
    }
}
