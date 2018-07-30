using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using Jibril.Extensions;
using Jibril.Preconditions;
using Jibril.Services.Entities;
using Jibril.Services.Entities.Tables;
using Jibril.Services.Level.Services;
using Microsoft.EntityFrameworkCore;
using Quartz.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jibril.Modules.Account
{
    public class Level : InteractiveBase
    {
        private readonly Calculate _calculate;

        public Level(Calculate calculate)
        {
            _calculate = calculate;
        }

        [Group("level")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(ChannelPermission.ManageRoles)]
        public class LevelAdmin : InteractiveBase
        {
            [Command("add", RunMode = RunMode.Async)]
            public async Task LevelAdd(uint level, [Remainder]string roleName)
            {
                if (roleName.IsNullOrWhiteSpace()) return;
                var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == roleName);
                if (role == null)
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Couldn't find a role by that name.", Color.Red.RawValue).Build());
                    return;
                }

                using (var db = new DbService())
                {
                    var check = await db.LevelRewards.FindAsync(Context.Guild.Id, level);
                    if (check != null)
                    {
                        await ReplyAsync(null, false,
                            new EmbedBuilder().Reply($"There's already a level reward on that level.", Color.Red.RawValue).Build());
                        return;
                    }

                    var data = new LevelReward
                    {
                        GuildId = Context.Guild.Id,
                        Level = level,
                        Role = role.Id,
                        Stackable = false
                    };
                    await db.LevelRewards.AddAsync(data);
                    await db.SaveChangesAsync();
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Added {role.Name} as a lvl{level} reward!", Color.Green.RawValue)
                            .Build());
                }
            }

            [Command("create", RunMode = RunMode.Async)]
            public async Task LevelCreate(uint level, [Remainder]string roleName)
            {
                if (roleName.IsNullOrWhiteSpace()) return;
                using (var db = new DbService())
                {
                    var role = await Context.Guild.CreateRoleAsync(roleName, GuildPermissions.None);
                    var data = new LevelReward
                    {
                        GuildId = Context.Guild.Id,
                        Level = level,
                        Role = role.Id,
                        Stackable = false
                    };
                    await db.LevelRewards.AddAsync(data);
                    await db.SaveChangesAsync();
                    await ReplyAsync(null, false,
                        new EmbedBuilder()
                            .Reply($"Successfully created and added {role.Name} as a level reward for level{level}!",
                                Color.Green.RawValue).Build());
                }
            }

            [Command("remove", RunMode = RunMode.Async)]
            public async Task LevelRemove(uint level)
            {
                using (var db = new DbService())
                {
                    var check = await db.LevelRewards.FindAsync(Context.Guild.Id, level);
                    if (check == null)
                    {
                        await ReplyAsync(null, false,
                            new EmbedBuilder().Reply($"There is no reward connected to that level.", Color.Red.RawValue)
                                .Build());
                        return;
                    }
                    var role = db.LevelRewards.First(x => x.GuildId == Context.Guild.Id && x.Level == level);
                    db.LevelRewards.Remove(role);
                    await db.SaveChangesAsync();
                    await ReplyAsync(null, false,
                        new EmbedBuilder()
                            .Reply($"Removed {Context.Guild.Roles.First(x => x.Id == role.Role).Name} from level rewards!", Color.Green.RawValue)
                            .Build());
                }
            }

            [Command("list", RunMode = RunMode.Async)]
            public async Task LevelAdd()
            {
                using (var db = new DbService())
                {
                    var roleList = new List<EmbedFieldBuilder>();
                    foreach (var x in db.LevelRewards.Where(x => x.GuildId == Context.Guild.Id).OrderBy(x => x.Level))
                    {
                        var role = Context.Guild.Roles.First(y => y.Id == x.Role);
                        var field = new EmbedFieldBuilder
                        {
                            IsInline = false,
                            Name = role.Name,
                            Value = x.Level
                        };
                        roleList.Add(field);
                    }
                    var author = new EmbedAuthorBuilder
                    {
                        IconUrl = Context.Guild.IconUrl,
                        Name = $"Level rewards for {Context.Guild.Name}"
                    };
                    var embed = new EmbedBuilder
                    {
                        Author = author,
                        Fields = roleList,
                        Color = Color.Purple
                    };
                    await ReplyAsync(null, false, embed.Build());
                }
            }
        }

        [Command("rank", RunMode = RunMode.Async)]
        [Ratelimit(1, 2, Measure.Seconds)]
        [RequiredChannel(339383206669320192)]
        public async Task RankAsync(SocketGuildUser user = null)
        {
            if (user == null) user = Context.User as SocketGuildUser;
            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(user);
                var rank = db.Accounts.CountAsync(x => x.TotalExp >= userdata.TotalExp);
                var total = db.Accounts.CountAsync(x => x.Active);
                await Task.WhenAll(rank, total);
                var nxtLevel = _calculate.GetNextLevelRequirement(userdata.Level);

                var author = new EmbedAuthorBuilder
                {
                    Name = user.GetName()
                };
                var embed = new EmbedBuilder
                {
                    Color = Color.Purple,
                    Author = author,
                    ThumbnailUrl = user.GetAvatar()
                };
                var level = new EmbedFieldBuilder
                {
                    Name = "Level",
                    IsInline = true,
                    Value = $"{userdata.Level}"
                };
                var exp = new EmbedFieldBuilder
                {
                    Name = "Exp",
                    IsInline = true,
                    Value = $"{userdata.Exp}/{nxtLevel}"
                };
                var ranking = new EmbedFieldBuilder
                {
                    Name = "Rank",
                    IsInline = true,
                    Value = $"{rank.Result}/{total.Result}"
                };

                embed.AddField(level);
                embed.AddField(exp);
                embed.AddField(ranking);
                await ReplyAsync("", false, embed.Build()).ConfigureAwait(false);
            }
        }

        [Command("top", RunMode = RunMode.Async)]
        [Ratelimit(1, 5, Measure.Seconds)]
        [RequiredChannel(339383206669320192)]
        public async Task LeaderboardAsync()
        {
            using (var db = new DbService())
            {
                var author = new EmbedAuthorBuilder
                {
                    Name = $"Level leaderboard for {Context.Guild.Name}",
                    IconUrl = Context.Guild.IconUrl
                };
                var embed = new EmbedBuilder
                {
                    Color = Color.Purple,
                    Author = author
                };
                var users = await db.Accounts.Where(x => x.Active && x.GuildId == Context.Guild.Id).OrderByDescending(account => account.TotalExp).Take(10).ToListAsync();
                var rank = 1;
                foreach (var x in users)
                {
                    var field = new EmbedFieldBuilder
                    {
                        IsInline = false,
                        Name = $"Rank: {rank}",
                        Value = $"<@{x.UserId}> - Level:{x.Level} - Total Exp:{x.TotalExp}"
                    };
                    embed.AddField(field);
                    rank++;
                }
                await ReplyAsync(null, false, embed.Build());
            }
        }

        [Command("rep", RunMode = RunMode.Async)]
        [Ratelimit(1, 2, Measure.Seconds)]
        [RequiredChannel(339383206669320192)]
        public async Task RepAsync(SocketGuildUser user = null)
        {
            using (var db = new DbService())
            {
                var cooldownCheckAccount = await db.GetOrCreateUserData(Context.User as SocketGuildUser);
                if (cooldownCheckAccount.RepCooldown.AddHours(18) >= DateTime.UtcNow)
                {
                    var timer = cooldownCheckAccount.RepCooldown.AddHours(18) - DateTime.UtcNow;
                    await ReplyAndDeleteAsync(null, false, new EmbedBuilder().Reply($"{Context.User.Mention} daily rep refresh in {timer.Humanize()}", Color.Red.RawValue).Build());
                    return;
                }
                var userdata = await db.GetOrCreateUserData(user);
                userdata.RepCooldown = DateTime.UtcNow;
                userdata.Rep = userdata.Rep + 1;
                await db.SaveChangesAsync();
                await ReplyAndDeleteAsync(null, false, new EmbedBuilder().Reply($"rewarded {user?.Mention} with a reputation point!", Color.Green.RawValue).Build());
            }
        }
    }
}