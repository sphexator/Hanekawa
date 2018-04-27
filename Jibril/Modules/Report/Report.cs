using System;
using Discord.Addons.Interactive;
using Discord.Commands;
using Jibril.Services;
using System.Linq;
using System.Threading.Tasks;
using Jibril.Modules.Report.Service;

namespace Jibril.Modules.Report
{
    public class Report : InteractiveBase
    {
        private readonly ReportService _service;
        public Report(ReportService service)
        {
            _service = service;
        }

        [Command("report", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        public async Task ReportGuild([Remainder] string text)
        {
            await Context.Message.DeleteAsync();
            var userdata = DatabaseService.UserData(Context.User).FirstOrDefault();
            if (userdata.Level <= 10) await ReplyAndDeleteAsync("You need to be level 10 or above to use the report system.", false, null, TimeSpan.FromSeconds(30));
            await _service.SendReport(Context.User, Context, text);
            await ReplyAndDeleteAsync("Report sent.", false, null, TimeSpan.FromSeconds(10));
        }

        [Command("report", RunMode = RunMode.Async)]
        [RequireContext(ContextType.DM)]
        public async Task ReportDm([Remainder] string text)
        {
            var userdata = DatabaseService.UserData(Context.User).FirstOrDefault();
            if (userdata.Level <= 10) await ReplyAsync("You need to be level 10 or above to use the report system.");
            await _service.SendReport(Context.User, Context, text);
            await ReplyAsync("Report sent.");
        }
    }
}