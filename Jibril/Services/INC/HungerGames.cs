using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Quartz;

namespace Jibril.Services.HungerGames
{
    public class HungerGames : IJob
    {
        private readonly DiscordSocketClient _client;
        private readonly List<ulong> _eventStartMsg;

        public HungerGames(DiscordSocketClient client)
        {
            _client = client;

            _client.ReactionAdded += AddParticipants;
        }

        public Task Execute(IJobExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public Task EventStart()
        {
            var _ = Task.Run(async() =>
            {
                var guild = _client.GetGuild(339370914724446208);
                var ch = guild.GetTextChannel(346429829316476928);
                var msg = await ch.SendMessageAsync("New HUNGER GAME event has started!\n\nTo enter, react to this message. \nThe first 50 users will be fighting for their life, on the quest to obtain ....");
                Emote.TryParse("<:rooree:362610653120299009>", out var emote);
                IEmote iemoteYes = emote;
                await msg.AddReactionAsync(iemoteYes);
                _eventStartMsg.Add(msg.Id);
            });
            return Task.CompletedTask;
        }

        private Task AddParticipants(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel ch, SocketReaction react)
        {
            var _ = Task.Run(async () =>
            {
                if (!_eventStartMsg.Contains(msg.Id)) return;
                if (msg.Value.Author.IsBot != true) return;
                if (react.User.Value.IsBot) return;
                if (react.Emote.Name != "rooree") return;
                
                // TODO: Add user to database as they react and return if they're already there
            });
            return Task.CompletedTask;
        }
    }
}
