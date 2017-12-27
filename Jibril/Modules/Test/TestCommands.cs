﻿using System.Linq;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;

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
    }
}