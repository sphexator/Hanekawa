using System;
using Discord.Addons.Interactive;
using Discord.Commands;
using Jibril.Services;
using System.Linq;
using System.Threading.Tasks;

namespace Jibril.Modules.Report
{
    public class Report : InteractiveBase
    {
        [Command("report", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        public async Task ReportGuild([Remainder] string text)
        {
            var userdata = DatabaseService.UserData(Context.User).FirstOrDefault();
            if (userdata.Level <= 10) await ReplyAndDeleteAsync("You need to be level 10 or above to use the report system.", false, null, TimeSpan.FromSeconds(30));

            await ReplyAsync("Report sent.");
        }

        [Command("report", RunMode = RunMode.Async)]
        [RequireContext(ContextType.DM)]
        public async Task ReportDm([Remainder] string text)
        {
            var userdata = DatabaseService.UserData(Context.User).FirstOrDefault();
            if (userdata.Level <= 10) await ReplyAndDeleteAsync("You need to be level 10 or above to use the report system.", false, null, TimeSpan.FromSeconds(30));

            await ReplyAsync("Report sent.");
        }
    }
}