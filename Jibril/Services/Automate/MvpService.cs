using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Extensions;
using Hanekawa.Services.Entities;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Hanekawa.Services.Automate
{
    public class MvpService : IJob
    {
        private readonly DiscordSocketClient _client;
        private readonly List<ulong> _channels = new List<ulong>();
        private ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, DateTime>> Cooldown { get; set; }
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, DateTime>>();

        public MvpService(DiscordSocketClient client)
        {
            _client = client;

            _client.MessageReceived += MessageCounter;

            _channels.Add(339371997802790913); //General
            _channels.Add(351861569530888202); //Tea-room
            _channels.Add(341904875363500032); //Gaming
            _channels.Add(353306001858101248); //Anime
            _channels.Add(382920381985456129); //tech
            _channels.Add(353306043373322252); //music

            _channels.Add(404633037884620802); //Test channel
        }
        // Message Reciever method
        private Task MessageCounter(SocketMessage msg)
        {
            var _ = Task.Run(async () =>
            {
                if (!(msg is SocketUserMessage message)) return;
                if (message.Source != MessageSource.User) return;
                if (!_channels.Contains(msg.Channel.Id)) return;
                var cd = CheckCooldownAsync(msg.Author as SocketGuildUser);
                if (cd == false) return;
                using (var db = new DbService())
                {
                    Console.WriteLine($"{DateTime.Now.ToLongTimeString()} | MVP SERVICE | +1 {msg.Author.Username}");
                    var user = await db.GetOrCreateUserData(msg.Author as SocketGuildUser);
                    user.MvpCounter = user.MvpCounter + 1;
                }
            });
            return Task.CompletedTask;
        }

        //Scheduled event
        public Task Execute(IJobExecutionContext context)
        {
            NewMvpUsers();
            return Task.CompletedTask;
        }

        private void NewMvpUsers()
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var guild = _client.GetGuild(339370914724446208);
                    var role = guild.Roles.FirstOrDefault(x => x.Name == "Kai Ni");
                    var oldMvps = role?.Members.ToList();
                    await db.Accounts.OrderBy(x => x.MvpCounter).Take(5).ToListAsync().ConfigureAwait(false);
                    var ma = await db.Accounts.OrderBy(x => x.MvpCounter).Take(5).ToListAsync().ConfigureAwait(false);
                    var newMvps = new List<IGuildUser>();
                    foreach (var x in ma)
                    {
                        var user = guild?.GetUser(x.UserId);
                        newMvps.Add(user);
                    }

                    try
                    {
                        var embed = MvpMessage(newMvps, oldMvps);
                        await guild.GetTextChannel(346429829316476928).SendMessageAsync("", false, embed.Build()).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Couldn't send new kai ni message\n" +
                                          $"{e}");
                    }

                    await Demote(oldMvps, role).ConfigureAwait(false);
                    await Promote(newMvps, role).ConfigureAwait(false);
                    await db.Accounts.ForEachAsync(x => x.MvpCounter = 0).ConfigureAwait(false);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
            });
        }

        private static async Task Demote(IEnumerable<IGuildUser> mvps, IRole role)
        {
            foreach (var x in mvps)
                try
                {
                    await x.RemoveRoleAsync(role);
                    await Task.Delay(1000);
                }
                catch
                {
                    //Ignore
                }
        }

        private static async Task Promote(IEnumerable<IGuildUser> mvps, IRole role)
        {
            foreach (var x in mvps)
                try
                {
                    await x.AddRoleAsync(role).ConfigureAwait(false);
                    await Task.Delay(1000);
                }
                catch
                {
                    //Ignore
                }
        }

        private static EmbedBuilder MvpMessage(IEnumerable<IGuildUser> newMvps, IEnumerable<IGuildUser> oldMvps)
        {
            var response = new List<string>();
            var outputp1 = oldMvps.Select(x => $"{x.Mention}").ToList();
            var outputp2 = newMvps.Select(y => $"{y.Mention}").ToList();

            for (var i = 0; i < 5; i++)
            {
                var content = $"{outputp1[i]} => {outputp2[i]} ";
                response.Add(content);
            }

            var desc = string.Join("\n", response);
            var embed = new EmbedBuilder
            {
                Title = "Kai Ni Update!",
                Color = Color.DarkPurple,
                Description = desc
            };
            return embed;
        }

        private bool CheckCooldownAsync(SocketGuildUser user)
        {
            var check = Cooldown.TryGetValue(user.Guild.Id, out var cds);
            if (!check)
            {
                Cooldown.TryAdd(user.Guild.Id, new ConcurrentDictionary<ulong, DateTime>());
                Cooldown.TryGetValue(user.Guild.Id, out cds);
                cds.TryAdd(user.Id, DateTime.UtcNow);
                return true;
            }

            var userCheck = cds.TryGetValue(user.Id, out var cd);
            if (!userCheck)
            {
                cds.TryAdd(user.Id, DateTime.UtcNow);
                return true;
            }

            if (!((DateTime.UtcNow - cd).TotalSeconds >= 60)) return false;
            cds.AddOrUpdate(user.Id, DateTime.UtcNow, (key, old) => old = DateTime.UtcNow);
            return true;
        }
    }
}