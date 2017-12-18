using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Jibril.Data.Variables;
using Jibril.Modules.Administration.Services;
using Jibril.Services.Common;

namespace Jibril.Modules.Administration
{
    public class Info : InteractiveBase
    {
        [Command("ginfo", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequireOwner]
        public async Task PostRules()
        {
            var guildid = Context.Guild.ToString();
            var rule = AdminDb.GetRules("339370914724446208");
            var faq = AdminDb.Getfaq("339370914724446208");

            var rng = new Random();

            var adminRole = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Admiral") as IRole;
            if (adminRole == null) throw new ArgumentNullException(nameof(adminRole));
            var modRole = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Secretary") as IRole;
            if (modRole == null) throw new ArgumentNullException(nameof(modRole));
            var trialRole = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Assistant Secretary") as IRole;
            if (trialRole == null) throw new ArgumentNullException(nameof(trialRole));

            var guild = Context.Guild as IGuild;
            var usrs = (await guild.GetUsersAsync()).ToArray();

            var admins = usrs.Where(x => x.RoleIds.Contains(adminRole.Id)).ToArray();
            var mod = usrs.Where(x => x.RoleIds.Contains(modRole.Id)).ToArray();
            var trial = usrs.Where(x => x.RoleIds.Contains(trialRole.Id)).ToArray();

            var adminstaffmen = string.Join("\n", admins.Select(x => x.Mention)
                .OrderBy(x => rng.Next()).Take(50));
            var modStaffmen = string.Join($"\n", mod.Select(x => x.Mention)
                .OrderBy(x => rng.Next()).Take(50));
            var trialStaffmen = string.Join("\n", trial.Select(x => x.Mention)
                .OrderBy(x => rng.Next()).Take(50));
            var levelRoles = $"__**Staff:**__ \n" +
                             $"{adminstaffmen}\n" +
                             $"\n" +
                             $"{modStaffmen}\n" +
                             $"{trialStaffmen}" +
                             $"\n" +
                             $"\n" +
                             $"__**Level roles & requirement:**__\n" +
                             $"Level 2 = Ship \n" +
                             $"Level 5 = Light Cruiser \n" +
                             $"Level 10 = Heavy Cruiser \n" +
                             $"Level 20 = Destroyer \n" +
                             $"Level 30 = Aircraft Carrier \n" +
                             $"Level 40 = Battleship \n" +
                             $"Level 50 = Aviation Cruiser \n" +
                             $"Level 65 = Aviation Battleship \n" +
                             $"Level 80 = Training Cruiser";
            // image
            await Context.Channel.SendFileAsync(@"Data/Images/Info/RULES.png");
            await ReplyAsync(rule[0]);
            // Image
            await Context.Channel.SendFileAsync(@"Data/Images/Info/FAQ.png");
            await ReplyAsync(faq[0]);
            await ReplyAsync($"{levelRoles}\n" +
                             $"\n" +
                             $"https://discord.gg/9tq4xNT");
            await Context.Message.DeleteAsync();
        }
    }
}