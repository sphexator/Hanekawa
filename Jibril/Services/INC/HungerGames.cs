using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Jibril.Services.INC.Database;
using Jibril.Services.INC.Generator;
using Quartz;
using Quartz.Util;
using SixLabors.ImageSharp;
using EventHandler = Jibril.Services.INC.Events.EventHandler;
using Image = SixLabors.ImageSharp.Image;

namespace Jibril.Services.INC
{
    public class HungerGames : IJob
    {
        private readonly SocketTextChannel _ch;
        private readonly DiscordSocketClient _client;
        private readonly List<ulong> _eventStartMsg;
        private bool _activeEvent;

        public HungerGames(DiscordSocketClient client)
        {
            _client = client;
            _client.ReactionAdded += AddParticipants;
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

        public async Task StartSignUp()
        {
            DatabaseHungerGame.GameSignUpStart();
            var msg = await _client.GetGuild(200265036596379648).GetTextChannel(404633092867751937).SendMessageAsync(
                "New HUNGER GAME event has started!\n\nTo enter, react to this message. \nThe first 25 users will be fighting for their life, on the quest to obtain ....");
            Emote.TryParse("<:rooree:362610653120299009>", out var emote);
            IEmote iemoteYes = emote;
            await msg.AddReactionAsync(iemoteYes);
            _eventStartMsg.Add(msg.Id);
        }

        public async Task StartEvent()
        {
            DatabaseHungerGame.GameStart();
            var users = DatabaseHungerGame.GetProfilEnumerable();
            string names = null;
            var row = 1;
            var numb = 1;
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

            await _client.GetGuild(200265036596379648).GetTextChannel(404633092867751937).SendMessageAsync(
                "Signup is closed and heres the following participants: \n" +
                $"{names}");
        }

        public async Task ContinueEvent()
        {
            var users = DatabaseHungerGame.GetProfilEnumerable();
            var images = ImageGenerator.GenerateEventImage(users);
            var output = new List<string>();
            var newUsers = DatabaseHungerGame.GetProfilEnumerable();
            foreach (var x in newUsers)
            {
                if (x.Player.Status == false) continue;
                var eventOutput = EventHandler.EventManager(x);
                if (eventOutput.IsNullOrWhiteSpace()) continue;
                var content = $"{x.Player.Name}: {eventOutput}";
                output.Add(content);
                DatabaseHungerGame.Stagger(x.Player.UserId);
            }
            var response = string.Join("\n", output);
            await _client.GetGuild(200265036596379648).GetTextChannel(404633092867751937).SendMessageAsync(response);
            foreach (var x in images)
                await _client.GetGuild(200265036596379648).GetTextChannel(404633092867751937).SendFileAsync(x, "");
        }

        private Task AddParticipants(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel ch, SocketReaction react)
        {
            var _ = Task.Run(async () =>
            {
                if (!_eventStartMsg.Contains(msg.Id)) return;
                if (msg.Value.Author.IsBot != true) return;
                if (react.User.Value.IsBot) return;
                if (react.Emote.Name != "rooree") return;
                var users = DatabaseHungerGame.GetUsers();
                if (users.Count >= 25) return;
                var check = DatabaseHungerGame.CheckExistingUser(react.User.Value);
                if (check != null) return;
                DatabaseHungerGame.EnterUser(react.User.Value);
                await SaveAvatar(react.User.Value);
            });
            return Task.CompletedTask;
        }

        private async Task SaveAvatar(IUser user)
        {
            var filePath = $"Services/INC/Cache/Avatar/{user.Id}.png";
            var httpclient = new HttpClient();
            HttpResponseMessage response;

            try
            {
                response = await httpclient.GetAsync(user.GetAvatarUrl());
            }
            catch
            {
                response = await httpclient.GetAsync(
                    "https://discordapp.com/assets/1cbd08c76f8af6dddce02c5138971129.png");
            }

            var inputStream = await response.Content.ReadAsStreamAsync();
            using (var img = Image.Load(inputStream))
            {
                img.Mutate(x => x.Resize(80, 80));
                img.Save(filePath);
            }
        }
    }
}