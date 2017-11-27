using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Preconditions;

namespace Jibril.Modules.Voice
{
    public class Move : InteractiveBase
    {
        [Command("move", RunMode = RunMode.Async)]
        [UserMustBeInVoice]
        [RequireContext(ContextType.Guild)]
        [Ratelimit(1, 2, Measure.Seconds)]
        public async Task MoveUser(SocketGuildUser user)
        {
            
        }
    }
}
