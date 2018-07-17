using Discord;
using Discord.WebSocket;
using Jibril.Extensions;
using Jibril.Services.INC.Generator;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Quartz.Util;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Jibril.Services.Entities;
using Jibril.Services.Entities.Tables;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Transforms;
using EventHandler = Jibril.Services.INC.Events.EventHandler;
using Image = SixLabors.ImageSharp.Image;

namespace Jibril.Services.INC
{
    public class HungerGames : IJob
    {
        private readonly SocketTextChannel _ch;
        private readonly DiscordSocketClient _client;
        private List<ulong> EventStartMsg { get; }
         = new List<ulong>();
        private List<ulong> Participants { get; }
            = new List<ulong>();
        private readonly bool ActiveEvent;

        // Test
        //private const ulong Guild = 431617676859932704;
        //private const ulong EventChannel = 441744578920448030;
        //private const ulong OutPutChannel = 441744578920448030;

        // Real
        private const ulong Guild = 339370914724446208;
        private const ulong EventChannel = 346429829316476928;
        private const ulong OutPutChannel = 441322970485620756;

        public HungerGames(DiscordSocketClient client)
        {
            _client = client;
            _client.ReactionAdded += AddParticipants;
            Directory.CreateDirectory("Services/INC/Cache/Avatar/");
            using (var db = new DbService())
            {
                var config = db.HungerGameConfigs.Find(Guild);
                ActiveEvent = config.Live;
                EventStartMsg.Add(config.MessageId);
                var users = db.HungerGameLives.ToList();
                foreach (var x in users)
                {
                    Participants.Add(x.UserId);
                }
            }
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
                using (var db = new DbService())
                {
                    var config = await db.HungerGameConfigs.FindAsync(Guild);

                    if (config.Live != true && config.SignupStage == false) await StartSignUp().ConfigureAwait(false);
                    else if (config.Live != true && config.SignupStage &&
                             config.SignupTime.AddHours(15) > DateTime.Now) return;
                    else if (config.Live) await ContinueEvent().ConfigureAwait(false);
                    else if (config.Live != true && config.SignupTime.AddHours(15) <= DateTime.Now) await StartEvent().ConfigureAwait(false);
                }
            });
            return Task.CompletedTask;
        }

        private async Task StartSignUp()
        {
            using (var db = new DbService())
            {
                var config = await db.HungerGameConfigs.FindAsync(Guild);

                var msg = await _client.GetGuild(Guild).GetTextChannel(EventChannel).SendMessageAsync(
                    "New HUNGER GAME event has started!\n\nTo enter, react to this message. \nThe first 25 users will be fighting for their life, on the quest to obtain ....");
                Emote.TryParse("<:rooree:430207965140877322>", out var emote);
                IEmote iemoteYes = emote;
                await msg.AddReactionAsync(iemoteYes);
                EventStartMsg.Add(msg.Id);

                config.SignupStage = true;
                config.SignupTime = DateTime.Now;
                config.MessageId = msg.Id;
                await db.SaveChangesAsync();
            }
        }

        private async Task StartEvent()
        {
            using (var db = new DbService())
            {
                AddDefaultCharacters();

                var config = await db.HungerGameConfigs.FindAsync(Guild);
                config.Live = true;
                config.Round = 0;
                config.SignupStage = false;
                await db.SaveChangesAsync();
                var users = db.HungerGameLives.ToList();
                var names =
                    $"{users[0].Name} - {users[1].Name} - {users[2].Name} - {users[3].Name} - {users[4].Name}\n" +
                    $"{users[5].Name} - {users[6].Name} - {users[7].Name} - {users[8].Name} - {users[9].Name}\n" +
                    $"{users[10].Name} - {users[11].Name} - {users[12].Name} - {users[13].Name} - {users[14].Name}\n" +
                    $"{users[15].Name} - {users[16].Name} - {users[17].Name} - {users[18].Name} - {users[19].Name}\n" +
                    $"{users[20].Name} - {users[21].Name} - {users[22].Name} - {users[23].Name} - {users[24].Name}\n";
                await _client.GetGuild(Guild).GetTextChannel(EventChannel).SendMessageAsync(
                    "Signup is closed and heres the following participants: \n" +
                    $"{names}");
            }
        }

        private async Task ContinueEvent()
        {
            using (var db = new DbService())
            {
                var rand = new Random();
                var users = await db.HungerGameLives.ToListAsync();
                var output = new List<string>();
                var ch = _client.GetGuild(Guild).GetTextChannel(OutPutChannel);
                foreach (var x in users)
                {
                    if (x.Status == false) continue;
                    var eventOutput = EventHandler.EventManager(x);
                    if (eventOutput.IsNullOrWhiteSpace()) continue;
                    var content = $"{x.Name.PadRight(20)} {eventOutput}";
                    output.Add(content);
                    var user = await db.HungerGameLives.FindAsync(x.UserId);
                    user.Fatigue = user.Fatigue + Convert.ToUInt32(rand.Next(10, 15));
                    user.Hunger = user.Hunger + Convert.ToUInt32(rand.Next(5, 10));
                    user.Thirst = user.Thirst + Convert.ToUInt32(rand.Next(10, 20));
                    user.Sleep = user.Sleep + Convert.ToUInt32(rand.Next(20, 30));
                    await db.SaveChangesAsync();
                }
                var response = string.Join("\n", output);
                var images = ImageGenerator.GenerateEventImage(await db.HungerGameLives.ToListAsync());
                images.Seek(0, SeekOrigin.Begin);
                await ch.SendFileAsync(images, "banner.png", $"Action Log\n```{response}\n```");

                var remaining = await db.HungerGameLives.Where(x => x.Status).ToListAsync();
                if (remaining.Count == 1)
                {
                    var chFinish = _client.GetGuild(Guild).GetTextChannel(EventChannel);
                    if (remaining.FirstOrDefault().UserId > 24)
                    {
                        var user = _client.GetUser(remaining.FirstOrDefault().UserId);
                        var userdata = await db.GetOrCreateUserData(user as SocketGuildUser);
                        userdata.Exp = userdata.Exp + 1000;
                        userdata.Credit = userdata.Credit + 1000;
                        userdata.CreditSpecial = userdata.CreditSpecial + 1000;
                        await db.SaveChangesAsync();
                        await chFinish.SendMessageAsync(
                            $"{user.Mention} is the new HungerGameLives champion and rewarded 1000 credit, event credit and exp!");
                    }
                    else
                    {
                        await chFinish.SendMessageAsync($"{remaining.FirstOrDefault().Name} is the new HungerGameLives champion!");
                    }
                    EndGame();
                }
                else
                {
                    var config = await db.HungerGameConfigs.FindAsync(Guild);
                    config.Round = config.Round + 1;
                    await db.SaveChangesAsync();
                }
            }
        }

        private Task AddParticipants(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel ch, SocketReaction react)
        {
            var _ = Task.Run(async () =>
            {
                if (!EventStartMsg.Contains(react.MessageId)) return;
                if (msg.Value.Author.IsBot != true) return;
                if (react.User.Value.IsBot) return;

                var user = react.User.Value as IGuildUser;

                if (react.Emote.Name != "rooree") return;
                if (Participants.Count >= 25) return;
                if (Participants.Contains(user.Id)) return;
                using (var db = new DbService())
                {
                    Participants.Add(user.Id);
                    var toAdd = AddUser(user.Id, user.Nickname ?? user.Username);
                    await db.HungerGameLives.AddAsync(toAdd);
                    await SaveAvatar(user);
                }
            });
            return Task.CompletedTask;
        }

        private static void AddDefaultCharacters()
        {
            using (var db = new DbService())
            {
                var totalUsers = db.HungerGameLives.ToList();
                if (totalUsers.Count >= 25) return;
                var remaining = 25 - totalUsers.Count;
                var users = db.HungerGameLives.ToList();
                var toAdd = new List<HungerGameLive>();
                for (var i = 0; i < remaining; i++)
                {
                    var duser = users[i];
                    var filePath = $"Services/INC/Cache/DefaultAvatar/{i}.png";
                    using (var img = Image.Load(filePath))
                    {
                        img.Mutate(x => x.Resize(80, 80));
                        img.Save($"Services/INC/Cache/Avatar/{users[i].UserId}.png");
                    }

                    var user = AddUser(duser.UserId, duser.Name);
                    toAdd.Add(user);
                }
                db.AddRange(toAdd);
            }
        }

        private static HungerGameLive AddUser(ulong id, string name)
        {
            var data = new HungerGameLive
            {
                UserId = id,
                Axe = 0,
                Food = 0,
                Bow = 0,
                Bleeding = false,
                Water = 0,
                Thirst = 0,
                TotalWeapons = 0,
                Pistol = 0,
                Sleep = 0,
                Stamina = 0,
                Status = false,
                Fatigue = 0,
                Health = 100,
                Hunger = 0,
                Name = name
            };
            return data;
        }

        private static async Task SaveAvatar(IUser user)
        {
            var filePath = $"Services/INC/Cache/Avatar/{user.Id}.png";
            var httpclient = new HttpClient();
            HttpResponseMessage response;

            try
            {
                response = await httpclient.GetAsync(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl());
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

        private static void ClearCache()
        {
            var cache = new DirectoryInfo("Services/INC/Cache/Avatar/");
            foreach (var file in cache.GetFiles())
                file.Delete();
        }

        private void EndGame()
        {
            using (var db = new DbService())
            {
                ClearCache();
                var cfg = db.HungerGameConfigs.Find(Guild);
                cfg.Live = false;
                cfg.Round = 0;
                db.SaveChanges();
                db.HungerGameLives.RemoveRange();
                db.SaveChanges();
                EventStartMsg.Clear();
                Participants.Clear();
            }
        }
    }
}