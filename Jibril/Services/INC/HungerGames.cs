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
            using (var db = new hanekawaContext())
            {
                var config = db.Hungergameconfig.Find(Guild);
                ActiveEvent = config.Live;
                EventStartMsg.Add(config.MsgId);
                var users = db.Hungergame.ToList();
                foreach (var x in users)
                {
                    Participants.Add(x.Userid);
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
                using (var db = new hanekawaContext())
                {
                    var config = await db.Hungergameconfig.FindAsync(Guild);

                    if (config.Live != true && config.Signupstage == false) await StartSignUp().ConfigureAwait(false);
                    else if (config.Live != true && config.Signupstage &&
                             config.SignupDuration.AddHours(15) > DateTime.Now) return;
                    else if (config.Live) await ContinueEvent().ConfigureAwait(false);
                    else if (config.Live != true && config.SignupDuration.AddHours(15) <= DateTime.Now) await StartEvent().ConfigureAwait(false);
                }
            });
            return Task.CompletedTask;
        }

        public async Task StartSignUp()
        {
            using (var db = new hanekawaContext())
            {
                var config = await db.Hungergameconfig.FindAsync(Guild);

                var msg = await _client.GetGuild(Guild).GetTextChannel(EventChannel).SendMessageAsync(
                    "New HUNGER GAME event has started!\n\nTo enter, react to this message. \nThe first 25 users will be fighting for their life, on the quest to obtain ....");
                Emote.TryParse("<:rooree:430207965140877322>", out var emote);
                IEmote iemoteYes = emote;
                await msg.AddReactionAsync(iemoteYes);
                EventStartMsg.Add(msg.Id);

                config.Signupstage = true;
                config.SignupDuration = DateTime.Now;
                config.MsgId = msg.Id;
                await db.SaveChangesAsync();
            }
        }

        public async Task StartEvent()
        {
            using (var db = new hanekawaContext())
            {
                AddDefaultCharacters();

                var config = await db.Hungergameconfig.FindAsync(Guild);
                config.Live = true;
                config.Round = 0;
                config.Signupstage = false;
                await db.SaveChangesAsync();
                var users = db.Hungergame.ToList();
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

        public async Task ContinueEvent()
        {
            using (var db = new hanekawaContext())
            {
                var rand = new Random();
                var users = await db.Hungergame.ToListAsync();
                var output = new List<string>();
                var ch = _client.GetGuild(Guild).GetTextChannel(OutPutChannel);
                foreach (var x in users)
                {
                    if (x.Status == false) continue;
                    var eventOutput = EventHandler.EventManager(x);
                    if (eventOutput.IsNullOrWhiteSpace()) continue;
                    var content = $"{x.Name.PadRight(20)} {eventOutput}";
                    output.Add(content);
                    var user = await db.Hungergame.FindAsync(x.Userid);
                    user.Fatigue = user.Fatigue + rand.Next(10, 15);
                    user.Hunger = user.Hunger + rand.Next(5, 10);
                    user.Thirst = user.Thirst + rand.Next(10, 20);
                    user.Sleep = user.Sleep + rand.Next(20, 30);
                    await db.SaveChangesAsync();
                }
                var response = string.Join("\n", output);
                var images = ImageGenerator.GenerateEventImage(await db.Hungergame.ToListAsync());
                images.Seek(0, SeekOrigin.Begin);
                await ch.SendFileAsync(images, "banner.png", $"Action Log\n```{response}\n```");

                var remaining = await db.Hungergame.Where(x => x.Status).ToListAsync();
                if (remaining.Count == 1)
                {
                    var chFinish = _client.GetGuild(Guild).GetTextChannel(EventChannel);
                    if (remaining.FirstOrDefault().Userid > 24)
                    {
                        var user = _client.GetUser(remaining.FirstOrDefault().Userid);
                        var userdata = await db.GetOrCreateUserData(user);
                        userdata.Xp = userdata.Xp + 1000;
                        userdata.Tokens = userdata.Tokens + 1000;
                        userdata.EventTokens = userdata.EventTokens + 1000;
                        await db.SaveChangesAsync();
                        await chFinish.SendMessageAsync(
                            $"{user.Mention} is the new HungerGame champion and rewarded 1000 credit, event credit and exp!");
                    }
                    else
                    {
                        await chFinish.SendMessageAsync($"{remaining.FirstOrDefault().Name} is the new HungerGame champion!");
                    }
                    EndGame();
                }
                else
                {
                    var config = await db.Hungergameconfig.FindAsync(Guild);
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
                using (var db = new hanekawaContext())
                {
                    Participants.Add(user.Id);
                    var toAdd = AddUser(user.Id, user.Nickname ?? user.Username);
                    await db.Hungergame.AddAsync(toAdd);
                    await SaveAvatar(user);
                }
            });
            return Task.CompletedTask;
        }

        private static void AddDefaultCharacters()
        {
            using (var db = new hanekawaContext())
            {
                var totalUsers = db.Hungergame.ToList();
                if (totalUsers.Count >= 25) return;
                var remaining = 25 - totalUsers.Count;
                var users = db.Hungergamedefault.ToList();
                var toAdd = new List<Hungergame>();
                for (var i = 0; i < remaining; i++)
                {
                    var duser = users[i];
                    var filePath = $"Services/INC/Cache/DefaultAvatar/{i}.png";
                    using (var img = Image.Load(filePath))
                    {
                        img.Mutate(x => x.Resize(80, 80));
                        img.Save($"Services/INC/Cache/Avatar/{users[i].Userid}.png");
                    }

                    var user = AddUser(duser.Userid, duser.Name);
                    toAdd.Add(user);
                }
                db.AddRange(toAdd);
            }
        }

        private static Hungergame AddUser(ulong id, string name)
        {
            var data = new Hungergame()
            {

                Userid = id,
                Arrows = 0,
                Axe = 0,
                Bandages = 0,
                Beans = 0,
                Bow = 0,
                Bullets = 0,
                Bleeding = false,
                Coke = 0,
                DamageTaken = 0,
                Water = 0,
                Ramen = 0,
                Redbull = 0,
                Thirst = 0,
                Totaldrink = 0,
                Totalfood = 0,
                Totalweapons = 0,
                Trap = 0,
                Pasta = 0,
                Pistol = 0,
                Sleep = 0,
                Stamina = 0,
                Status = false,
                Fatigue = 0,
                Fish = 0,
                Health = 100,
                Hunger = 0,
                Name = name,
                Mountaindew = 0
            };
            return data;
        }

        private async Task SaveAvatar(IUser user)
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

        public void EndGame()
        {
            using (var db = new hanekawaContext())
            {
                ClearCache();
                var cfg = db.Hungergameconfig.Find(Guild);
                cfg.Live = false;
                cfg.Round = 0;
                db.SaveChanges();
                db.Hungergame.RemoveRange();
                db.SaveChanges();
                EventStartMsg.Clear();
                Participants.Clear();
            }
        }
    }
}