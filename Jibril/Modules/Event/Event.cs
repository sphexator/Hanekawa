using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;
using Jibril.Preconditions;

namespace Jibril.Modules.Event
{
    public class Event : InteractiveBase
    {
        [Command("ping")]
        [RequiredChannel(346429829316476928)]
        [RequireContext(ContextType.Guild)]
        [RequireRole(379324308436156416)]
        [Ratelimit(1, 20, Measure.Hours)]
        public async Task PingTask()
        {
            var msg = await ReplyAsync("@here");
            await msg.DeleteAsync();
        }
    }
}
