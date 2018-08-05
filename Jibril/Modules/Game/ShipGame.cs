using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Preconditions;
using Jibril.Services.Games.ShipGame;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jibril.Modules.Game
{
    public class ShipGame : InteractiveBase
    {
        private readonly ShipGameService _gameService;

        public ShipGame(ShipGameService gameService)
        {
            _gameService = gameService;
        }

        [Command("search", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [Ratelimit(1, 1, Measure.Seconds)]
        [RequiredChannel(346429281314013184)]
        public async Task ShipGameSearchAsync()
        {
            await ReplyAsync(null, false, (await _gameService.SearchAsync(Context)).Build());
        }

        [Command("attack", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequiredChannel(346429281314013184)]
        public async Task AttackGameAsync()
        {
            await _gameService.AttackAsync(Context);
        }

        [Command("duel", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequiredChannel(346429281314013184)]
        public async Task AttackGameAsync(SocketGuildUser user, uint bet = 0)
        {
            if (user == Context.User) return;
            var msg = bet == 0
                ? $"{user.Mention}, {Context.User.Mention} has challenged you to a duel, do you accept? (y/n)"
                : $"{user.Mention}, {Context.User.Mention} has challenged you to a duel with ${bet} at stake, do you accept? (y/n)";
            await ReplyAsync(msg);
            var response = await NextMessageAsync(new EnsureFromUserCriterion(user.Id), TimeSpan.FromSeconds(30));
            if (response.Content.ToLower() != "y") await ReplyAsync("Duel cancelled");
            await _gameService.AttackAsync(Context, user, bet);
        }
    }
}