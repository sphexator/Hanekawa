using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Jibril.Services;
using Jibril.Services.Level;

namespace Jibril.Modules.Test
{
    public class TestCommands : InteractiveBase
    {
        [Command("role")]
        [RequireOwner]
        public async Task GetRoleId(string name)
        {
            var role = Context.Guild.Roles.First(x => x.Name == name);
            if (role == null) return;
            await ReplyAsync($"{role.Name} - {role.Id}");
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

            var embed = I_am_infamous.MVPMessage(oldMvps, oldMvps);
            await ReplyAsync("", false, embed.Build());
        }
    }
}