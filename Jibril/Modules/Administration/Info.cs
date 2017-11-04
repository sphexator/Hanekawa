using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Jibril.Modules.Administration.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Jibril.Modules.Administration
{
    public class Info : InteractiveBase
    {
        [Command("ginfo", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task PostRules()
        {
            var rule = AdminDb.GetRules(Context.Guild.Id.ToString());
            await ReplyAsync(rule.ToString());
        }
    }
}
