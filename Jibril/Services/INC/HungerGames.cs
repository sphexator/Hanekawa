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
        private SocketGuild _guild;
        private SocketTextChannel _ch;

        public HungerGames(DiscordSocketClient client)
        {
            _client = client;
            _client.ReactionAdded += AddParticipants;

            _guild = _client.GetGuild(339370914724446208);
            _ch = _guild.GetTextChannel(346429829316476928);
            _ch.GetMessagesAsync();
        }

        public Task Execute(IJobExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public Task InitializeTask()
        {
            var _ = Task.Run(async () =>
            {
                var config = DatabaseHungerGame.GetConfig().FirstOrDefault() ?? throw new ArgumentNullException(
                                 $"DatabaseHungerGame.GetConfig().FirstOrDefault()");

                if (config.Live != true) await StartSignUp();
                if (config.Live != true && config.SignUpStage &&
                    config.SignupDuration.AddHours(23) > DateTime.UtcNow) return;
                if (config.Live) await ContinueEvent();             
                if (config.Live != true && config.SignupDuration.AddHours(23) <= DateTime.UtcNow) await StartEvent();
            });
            return Task.CompletedTask;
        }

        private async Task StartSignUp()
        {
            DatabaseHungerGame.GameSignUpStart();
            var guild = _client.GetGuild(339370914724446208);
            var ch = guild.GetTextChannel(346429829316476928);
            var msg = await ch.SendMessageAsync("New HUNGER GAME event has started!\n\nTo enter, react to this message. \nThe first 50 users will be fighting for their life, on the quest to obtain ....");
            Emote.TryParse("<:rooree:362610653120299009>", out var emote);
            IEmote iemoteYes = emote;
            await msg.AddReactionAsync(iemoteYes);
            _eventStartMsg.Add(msg.Id);
        }

        private async Task StartEvent()
        {
            DatabaseHungerGame.GameStart();
            var users = DatabaseHungerGame.GetProfilEnumerable();
            string names = null;
            var row = 1;
            int numb = 1;
            foreach (var x in users)
            {
                if (numb == 1)
                {
                    names = string.Join("", x.Player.Name);
                    numb++;
                }
                if (numb == 5 * row && numb != 1)
                {
                    names = string.Join($"\n", x.Player.Name);
                    numb++;
                    row++;
                }
                if (numb != 5 * row && numb != 1)
                {
                    names = string.Join($" - ", x.Player.Name);
                    numb++;
                }
            }

            await _ch.SendMessageAsync("Signup is closed and heres the following participants: \n" +
                                       $"{names}");
        }

        private async Task ContinueEvent()
        {
            var users = DatabaseHungerGame.GetProfilEnumerable();
            foreach (var x in users)
            {
                var action = Events.EventHandler.EventManager(x);
            }
        }

        private Task AddParticipants(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel ch, SocketReaction react)
        {
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
            });
            return Task.CompletedTask;
        }
    }
}
