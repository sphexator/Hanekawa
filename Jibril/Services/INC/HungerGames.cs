using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Jibril.Modules.Gambling.Services;
using Jibril.Services.INC.Database;
using Jibril.Services.INC.Generator;
using Jibril.Services.Level.Services;
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
        private const ulong Guild = 200265036596379648;
        private const ulong EventChannel = 404633092867751937;
        private const ulong OutPutChannel = 404633092867751937;

        public HungerGames(DiscordSocketClient client)
        {
            _client = client;
            _client.ReactionAdded += AddParticipants;
            Directory.CreateDirectory("Services/INC/Cache/Avatar/");
            
            var config = DatabaseHungerGame.GetConfig().FirstOrDefault() ?? throw new ArgumentNullException(
                             $"DatabaseHungerGame.GetConfig().FirstOrDefault()");
            _activeEvent = config.Live;
            var fuckingBullShit = new List<ulong> {config.MsgId};
            _eventStartMsg = fuckingBullShit;
        }

        public Task Execute(IJobExecutionContext context)
        {
            try
            {
                InitializeTask();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return Task.CompletedTask;
        }

        private Task InitializeTask()
        {
            var _ = Task.Run(async () =>
            {
                var config = DatabaseHungerGame.GetConfig().FirstOrDefault() ?? throw new ArgumentNullException(
                                 $"DatabaseHungerGame.GetConfig().FirstOrDefault()");

                if (config.Live != true && config.SignUpStage == false) await StartSignUp().ConfigureAwait(false);
                if (config.Live != true && config.SignUpStage &&
                    config.SignupDuration.AddMinutes(1) > DateTime.Now) return;
                if (config.Live) await ContinueEvent().ConfigureAwait(false);
                if (config.Live != true && config.SignupDuration.AddMinutes(1) <= DateTime.Now) await StartEvent().ConfigureAwait(false);
            });
            return Task.CompletedTask;
        }

        public async Task StartSignUp()
        {
            DatabaseHungerGame.GameSignUpStart();
            var msg = await _client.GetGuild(Guild).GetTextChannel(EventChannel).SendMessageAsync(
                "New HUNGER GAME event has started!\n\nTo enter, react to this message. \nThe first 25 users will be fighting for their life, on the quest to obtain ....");
            Emote.TryParse("<:rooree:430207965140877322>", out var emote);
            IEmote iemoteYes = emote;
            await msg.AddReactionAsync(iemoteYes);
            _eventStartMsg.Add(msg.Id);
            DatabaseHungerGame.StoreMsgId(msg.Id);
        }

        public async Task StartEvent()
        {
            var totalUsers = DatabaseHungerGame.GetTotalUsers();
            AddDefaultCharacters(totalUsers);
            DatabaseHungerGame.GameStart();
            var users = DatabaseHungerGame.GetProfilEnumerable();
            var names =
                $"{users[0].Player.Name} - {users[1].Player.Name} - {users[2].Player.Name} - {users[3].Player.Name} - {users[4].Player.Name}\n" +
                $"{users[6].Player.Name} - {users[7].Player.Name} - {users[7].Player.Name} - {users[8].Player.Name} - {users[9].Player.Name}\n" +
                $"{users[10].Player.Name} - {users[11].Player.Name} - {users[12].Player.Name} - {users[13].Player.Name} - {users[14].Player.Name}\n" +
                $"{users[15].Player.Name} - {users[16].Player.Name} - {users[17].Player.Name} - {users[18].Player.Name} - {users[19].Player.Name}\n" +
                $"{users[20].Player.Name} - {users[21].Player.Name} - {users[22].Player.Name} - {users[23].Player.Name} - {users[24].Player.Name}\n";
            await _client.GetGuild(Guild).GetTextChannel(EventChannel).SendMessageAsync(
                "Signup is closed and heres the following participants: \n" +
                $"{names}");
        }

        public async Task ContinueEvent()
        {
            var users = DatabaseHungerGame.GetProfilEnumerable();
            var output = new List<string>();
            var ch = _client.GetGuild(Guild).GetTextChannel(OutPutChannel);
            foreach (var x in users)
            {
                if (x.Player.Status == false) continue;
                var userCheck = DatabaseHungerGame.CheckExistingUser(x.Player.UserId).FirstOrDefault();
                if (userCheck.Status == false) continue;
                var eventOutput = EventHandler.EventManager(x);
                if (eventOutput.IsNullOrWhiteSpace()) continue;
                var content = $"{x.Player.Name.PadRight(20)} {eventOutput}";
                output.Add(content);
                DatabaseHungerGame.Stagger(x.Player.UserId);
            }
            var response = string.Join("\n", output);

            var newUsers = DatabaseHungerGame.GetProfilEnumerable();
            var images = ImageGenerator.GenerateEventImage(newUsers);
            images.Seek(0, SeekOrigin.Begin);
            await ch.SendFileAsync(images, "banner.png", $"Action Log\n```{response}\n```");

            var remaining = DatabaseHungerGame.GetUsers();
            if (remaining.Count == 1)
            {
                var chFinish = _client.GetGuild(Guild).GetTextChannel(EventChannel);
                if (remaining.FirstOrDefault().UserId > 24)
                {
                    var user = _client.GetUser(remaining.FirstOrDefault().UserId);
                    LevelDatabase.AddExperience(user, 1000, 1000);
                    GambleDB.AddEventCredit(user, 1000);
                    await chFinish.SendMessageAsync(
                        $"{user.Mention} is the new HungerGame champion and rewarded 1000 credit, event credit and exp!");
                }
                else
                {
                    await chFinish.SendMessageAsync($"{remaining.FirstOrDefault().Name} is the new HungerGame champion!");
                }
                EndGame();
            }
            else DatabaseHungerGame.GameRoundIncrease();
        }
        
        private Task AddParticipants(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel ch, SocketReaction react)
        {
            var _ = Task.Run(async () =>
            {
                if (!_eventStartMsg.Contains(react.MessageId)) return;
                if (msg.Value.Author.IsBot != true) return;
                if (react.User.Value.IsBot) return;
                if (react.Emote.Name != "rooree") return;
                var users = DatabaseHungerGame.GetUsers();
                if (users.Count >= 25) return;
                var check = DatabaseHungerGame.CheckExistingUser(react.User.Value);
                if (check == null) return;
                DatabaseHungerGame.EnterUser(react.User.Value);
                await SaveAvatar(react.User.Value);
            });
            return Task.CompletedTask;
        }

        private static void AddDefaultCharacters(ICollection result)
        {
            if (result.Count >= 25) return;
            var remaining = 25 - result.Count;
            var users = DatabaseHungerGame.GetDefaultUsers();
            for (var i = 0; i < remaining; i++)
            {
                var filePath = $"Services/INC/Cache/DefaultAvatar/{i}.png";
                using (var img = Image.Load(filePath))
                {
                    img.Mutate(x => x.Resize(80, 80));
                    img.Save($"Services/INC/Cache/Avatar/{users[i].Userid}.png");
                }

                DatabaseHungerGame.EnterUser(users[i].Userid, users[i].Name);
            }
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
        private  static void ClearCache()
        {
            var cache = new DirectoryInfo("Services/INC/Cache/Avatar/");
            foreach (var file in cache.GetFiles())
                file.Delete();
        }
        private static void EndGame()
        {
            ClearCache();
            DatabaseHungerGame.GameEnd();
            DatabaseHungerGame.ClearTable();
        }
    }
}