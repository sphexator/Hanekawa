using Discord;
using Discord.WebSocket;
using Discord.Addons.Interactive;
using Discord.Commands;
using Jibril.Modules.Administration.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Jibril.Services.Common;
using Jibril.Data.Variables;

namespace Jibril.Modules.Administration
{
    public class Info : InteractiveBase
    {
        [Command("ginfo", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task PostRules()
        {
            var guildid = Context.Guild.ToString();
            var rule = AdminDb.GetRules("339370914724446208");
            var faq = AdminDb.Getfaq("339370914724446208");

            var rng = new Random();

            var adminRole = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Admiral") as IRole;
            var ModRole =   Context.Guild.Roles.FirstOrDefault(x => x.Name == "Secretary") as IRole;
            var TrialRole = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Assistant Secretary") as IRole;

            var guild = Context.Guild as IGuild;
            var usrs = (await guild.GetUsersAsync()).ToArray();

            var admins =    usrs.Where(x => x.RoleIds.Contains(adminRole.Id)).ToArray();
            var Mod =       usrs.Where(x => x.RoleIds.Contains(ModRole.Id))  .ToArray();
            var Trial =     usrs.Where(x => x.RoleIds.Contains(TrialRole.Id)).ToArray();

            var Adminstaff = string.Join("\n ", admins
                .OrderBy(x => rng.Next()).Take(50));

            var ModStaff = string.Join("\n ", Mod
                .OrderBy(x => rng.Next()).Take(50));

            var TrialStaff = string.Join("\n ", Trial
                .OrderBy(x => rng.Next()).Take(50));

            var embed = EmbedGenerator.DefaultEmbed($"Staff: \n" +
                $"{Adminstaff}\n" +
                $"\n" +
                $"{ModStaff}\n" +
                $"\n" +
                $"{TrialStaff}" +
                $"\n"
                , Colours.DefaultColour);

            // image
            await ReplyAsync(rule[0]);
            // Image
            await ReplyAsync(faq[0]);
            await ReplyAsync("", false, embed.Build());
        }
    }
}
