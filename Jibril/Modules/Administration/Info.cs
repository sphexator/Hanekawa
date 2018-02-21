using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Jibril.Modules.Administration.Services;

namespace Jibril.Modules.Administration
{
    public class Info : InteractiveBase
    {
        [Command("setrules")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetRules([Remainder] string rules)
        {
            AdminDb.SetRules(rules);
            var msgid = AdminDb.GetRulesMsgId();
            var msg = await Context.Guild.GetTextChannel(339370914724446208).GetMessageAsync(msgid[0]) as IUserMessage;
            await msg.ModifyAsync(x => x.Content = rules);
            await ReplyAsync("Successfully updated rules");
        }

        [Command("setfaq1")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetFaqOne([Remainder] string faq)
        {
            AdminDb.SetFaqOne(faq);
            var msgid = AdminDb.GetFaqOneMsgId();
            var msg = await Context.Guild.GetTextChannel(339370914724446208).GetMessageAsync(msgid[0]) as IUserMessage;
            await msg.ModifyAsync(x => x.Content = faq);
            await ReplyAsync("Successfully updated FAQ one");
        }

        [Command("setfaq2")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetFaqTwo([Remainder] string faq)
        {
            AdminDb.SetFaqTwo(faq);
            var msgid = AdminDb.GetFaqTwoMsgId();
            var msg = await Context.Guild.GetTextChannel(339370914724446208).GetMessageAsync(msgid[0]) as IUserMessage;
            await msg.ModifyAsync(x => x.Content = faq);
            await ReplyAsync("Successfully updated FAQ Two");
        }

        [Command("updstaff")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task UpdateStaffList()
        {
            var rng = new Random();
            var msgid = AdminDb.GetStaffMsgId();
            if (!(Context.Guild.Roles.FirstOrDefault(x => x.Name == "Admiral") is IRole adminRole)) throw new ArgumentNullException(nameof(adminRole));
            if (!(Context.Guild.Roles.FirstOrDefault(x => x.Name == "Secretary") is IRole modRole)) throw new ArgumentNullException(nameof(modRole));
            if (!(Context.Guild.Roles.FirstOrDefault(x => x.Name == "Assistant Secretary") is IRole trialRole)) throw new ArgumentNullException(nameof(trialRole));

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

            var content = $"__**Staff:**__\n" +
                          $"{adminstaffmen}\n" +
                          $"\n" +
                          $"{modStaffmen}\n" +
                          $"{trialStaffmen}";

            var ch = await guild.GetTextChannelAsync(339370914724446208);
            var msg = await ch.GetMessageAsync(msgid[0]) as IUserMessage;

            await msg.ModifyAsync(x => x.Content = content);

        }

        [Command("updinvite")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task UpdateInviteLink()
        {
            var msgid = AdminDb.GetLevelInviteMsgId();
            var invite = await Context.Guild.DefaultChannel.CreateInviteAsync();

            var content = $"{LevelRoles}\n" +
                          $"\n" +
                          $"{invite.Url}";
            var msg = await Context.Guild.GetTextChannel(339370914724446208).GetMessageAsync(msgid[0]) as IUserMessage;
            await msg.ModifyAsync(x => x.Content = content);
        }

        [Command("ginfo", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequireOwner]
        public async Task PostRules()
        {
            var rule = AdminDb.GetRules();
            var faq = AdminDb.Getfaq();
            var faq2 = AdminDb.GetfaqTwo();

            var rng = new Random();

            if (!(Context.Guild.Roles.FirstOrDefault(x => x.Name == "Admiral") is IRole adminRole)) throw new ArgumentNullException(nameof(adminRole));
            if (!(Context.Guild.Roles.FirstOrDefault(x => x.Name == "Secretary") is IRole modRole)) throw new ArgumentNullException(nameof(modRole));
            if (!(Context.Guild.Roles.FirstOrDefault(x => x.Name == "Assistant Secretary") is IRole trialRole)) throw new ArgumentNullException(nameof(trialRole));

            var guild = Context.Guild as IGuild;
            var usrs = (await guild.GetUsersAsync()).ToArray();

            var admins = usrs.Where(x => x.RoleIds.Contains(adminRole.Id)).ToArray();
            var mod = usrs.Where(x => x.RoleIds.Contains(modRole.Id)).ToArray();
            var trial = usrs.Where(x => x.RoleIds.Contains(trialRole.Id)).ToArray();

            var adminstaffmen = string.Join("\n", admins.Select(x => x.Mention)
                .OrderBy(x => rng.Next()).Take(50));
            var modStaffmen = string.Join("\n", mod.Select(x => x.Mention)
                .OrderBy(x => rng.Next()).Take(50));
            var trialStaffmen = string.Join("\n", trial.Select(x => x.Mention)
                .OrderBy(x => rng.Next()).Take(50));
            var staffmsg = "__**Staff:**__ \n" +
                           $"{adminstaffmen}\n" +
                           "\n" +
                           $"{modStaffmen}\n" +
                           $"{trialStaffmen}";
            // image
            await Context.Channel.SendFileAsync(@"Data/Images/Info/RULES.png");
            var ruleMsg = await ReplyAsync(rule[0]);
            // Image
            await Context.Channel.SendFileAsync(@"Data/Images/Info/FAQ.png");
            var faqMsg1 = await ReplyAsync(faq[0]);
            var faqMsg2 = await ReplyAsync(faq2[0]);
            var staffMsg = await ReplyAsync(staffmsg);
            var levelInvite = await ReplyAsync($"{LevelRoles}\n" +
                             "\n" +
                             "https://discord.gg/9tq4xNT");
            await Context.Message.DeleteAsync();
            AdminDb.SetRulesMsgId(ruleMsg.Id);
            AdminDb.SetFaqOneMsgId(faqMsg1.Id);
            AdminDb.SetFaqTwoMsgId(faqMsg2.Id);
            AdminDb.SetStaffMsgId(staffMsg.Id);
            AdminDb.SetLevelInviteMsgId(levelInvite.Id);
        }

        private const string LevelRoles = "__** Level roles & requirement:**__\n" +
                                         "Level 2 = Ship \n" +
                                         "Level 5 = Light Cruiser \n" +
                                         "Level 10 = Heavy Cruiser \n" +
                                         "Level 20 = Destroyer \n" +
                                         "Level 30 = Aircraft Carrier \n" +
                                         "Level 40 = Battleship \n" +
                                         "Level 50 = Aviation Cruiser \n" +
                                         "Level 65 = Aviation Battleship \n" +
                                         "Level 80 = Training Cruiser";
    }
}