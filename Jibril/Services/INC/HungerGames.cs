using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Jibril.Services.INC.Database;
using Quartz;

namespace Jibril.Services.INC
{
    public class HungerGames : IJob
    {
        private readonly DiscordSocketClient _client;

        private List<ulong> _eventStartMsg;
        private bool _activeEvent;

        public HungerGames(DiscordSocketClient client)
        {
            throw new NotImplementedException();
            /*
            _client = client;
            _client.ReactionAdded += AddParticipants;

            var guild = _client.GetGuild(339370914724446208);
            var ch = guild.GetTextChannel(346429829316476928);
            ch.GetMessagesAsync();
            */
        }

        public Task Execute(IJobExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public Task InitializeTask()
        {
            throw new NotImplementedException();
            /*
            var _ = Task.Run(async () =>
            {
                var config = DatabaseHungerGame.GetConfig().FirstOrDefault() ?? throw new ArgumentNullException(
                                 $"DatabaseHungerGame.GetConfig().FirstOrDefault()");
                if (config.Live != true) await StartSignUp();
                if (config.Live) await ContinueEvent();
                var difference = DateTime.Compare(config.SignupDuration, DateTime.UtcNow);
                if (config.Live != true && (config.SignupDuration.ToString() == "0001-01-01 00:00:00" ||
                                            config.SignupDuration.AddHours(23) <= DateTime.UtcNow && difference < 0 ||
                                            difference >= 0))
                {
                    await StartEvent();
                }
            });
            return Task.CompletedTask;
            */
        }

        private Task StartSignUp()
        {
            throw new NotImplementedException();
            /*
            var guild = _client.GetGuild(339370914724446208);
            var ch = guild.GetTextChannel(346429829316476928);
            var msg = await ch.SendMessageAsync("New HUNGER GAME event has started!\n\nTo enter, react to this message. \nThe first 50 users will be fighting for their life, on the quest to obtain ....");
            Emote.TryParse("<:rooree:362610653120299009>", out var emote);
            IEmote iemoteYes = emote;
            await msg.AddReactionAsync(iemoteYes);
            _eventStartMsg.Add(msg.Id);
            */
        }

        private Task StartEvent()
        {
            throw new NotImplementedException();
            /*
            var users = DatabaseHungerGame.GetProfilEnumerable();
            foreach (var x in users)
            {
                var action = Events.EventHandler.EventManager(x);
            }
            */
        }

        private Task ContinueEvent()
        {
            throw new NotImplementedException();
        }

        private Task AddParticipants(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel ch, SocketReaction react)
        {
            throw new NotImplementedException();
            /*
            var _ = Task.Run(() =>
            {
                if (!_eventStartMsg.Contains(msg.Id)) return;
                if (msg.Value.Author.IsBot != true) return;
                if (react.User.Value.IsBot) return;
                if (react.Emote.Name != "rooree") return;
                var users = DatabaseHungerGame.GetUsers();
                var check = DatabaseHungerGame.CheckExistingUser(react.User.Value);
                if (users.Count >= 50 && check != null) return;
                DatabaseHungerGame.EnterUser(react.User.Value);
                // TODO: Add user to database as they react and return if they're already there
            });
            return Task.CompletedTask;
            */
        }

        public IUser GetUser(ulong id)
        {
            var user = _client.GetUser(id);
            return user;
        }
    }
}
