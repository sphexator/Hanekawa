using Discord;
using Discord.Commands;
using Discord.Addons.Preconditions;
using Jibril.Preconditions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Jibril.Modules.Game
{
    public class NPC : ModuleBase<SocketCommandContext>
    {
        [Command("search", RunMode = RunMode.Async)]
        [Alias("find", "radar")]
        [RequiredChannel(346429281314013184)]
        [Ratelimit(12, 1, Measure.Minutes, false, false)]
        public async Task FindNPC()
        {

        }
    }
}
