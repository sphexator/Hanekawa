using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Preconditions;
using Jibril.Services.Loot;

namespace Jibril.Modules.Administration
{
    public class Crates : InteractiveBase
    {
        private readonly LootCrates _drops;
        public Crates(LootCrates drops)
        {
            _drops = drops;
        }

        [Command("drop", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireContext(ContextType.Guild)]
        [Ratelimit(3, 30, Measure.Minutes)]
        public async Task Drop()
        {
            await _drops.SpawnCrate(Context.Channel as SocketTextChannel, Context.User as SocketGuildUser);
            await Context.Message.DeleteAsync();
        }
    }
}
