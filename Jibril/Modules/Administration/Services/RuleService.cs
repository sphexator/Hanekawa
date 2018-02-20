using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Jibril.Modules.Administration.Services
{
    public class RuleService
    {
        private DiscordSocketClient _client;

        public RuleService(DiscordSocketClient client)
        {
            _client = client;

            _client.GuildMemberUpdated += UpdateStaff;
        }

        private Task UpdateStaff(SocketGuildUser oldUser, SocketGuildUser newUser)
        {
            throw new NotImplementedException();
        }

        public static async Task UpdateRules()
        {

        }

        public static async Task UpdateFAQ(IGuild guild)
        {
            const ulong msgid1 = 0;
            const ulong msgid2 = 0;
            var ch = await guild.GetTextChannelAsync(339370914724446208);
            var msg1 = await ch.GetMessageAsync(msgid1) as IUserMessage;
            var msg2 = await ch.GetMessageAsync(msgid2) as IUserMessage;
        }

        public static async Task UpdateStaff(IGuild guild)
        {
            const ulong msgid = 0;
            var rng = new Random();

            if (!(guild.Roles.FirstOrDefault(x => x.Name == "Admiral") is IRole adminRole)) throw new ArgumentNullException(nameof(adminRole));
            if (!(guild.Roles.FirstOrDefault(x => x.Name == "Secretary") is IRole modRole)) throw new ArgumentNullException(nameof(modRole));
            if (!(guild.Roles.FirstOrDefault(x => x.Name == "Assistant Secretary") is IRole trialRole)) throw new ArgumentNullException(nameof(trialRole));

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
            var msg = await ch.GetMessageAsync(msgid) as IUserMessage;

            await msg.ModifyAsync(x => x.Content = content);
        }
    }
}
