using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Extensions.Embed;
using Microsoft.EntityFrameworkCore;
using Qmmands;

namespace Hanekawa.Bot.Modules.Level
{
    public partial class Level
    {
        [Name("Exp Multiplier")]
        [Command("exp multiplier", "exp multi")]
        [Description("Checks what the current exp multiplier is")]
        [Remarks("exp multi")]
        [RequiredChannel]
        public async Task ExpMultiplier()
        {
            await Context.ReplyAsync($"Current multiplier is: {_exp.GetMultiplier(Context.Guild.Id)}");
        }

        [Name("Exp Multiplier")]
        [Command("exp multiplier", "exp multi")]
        [Description("Sets a new exp multiplier permanently")]
        [Remarks("exp multi 2.1")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task ExpMultiplier(double multiplier)
        {
            using (var db = new DbService())
            {
                var old = _exp.GetMultiplier(Context.Guild.Id);
                var cfg = await db.GetOrCreateLevelConfigAsync(Context.Guild);
                cfg.ExpMultiplier = multiplier;
                await db.SaveChangesAsync();
                _exp.AdjustMultiplier(Context.Guild.Id, multiplier);
                await Context.ReplyAsync($"Changed exp multiplier from {old} to {multiplier}", Color.Green.RawValue);
            }
        }

        [Name("Add Experience")]
        [Command("add exp")]
        [Description("Give a certain amount of experience to a one or more users")]
        [Remarks("exp give 100 @bob#0000")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task AddExp(int exp, params SocketGuildUser[] users)
        {
            if (exp == 0) return;
            if (exp < 0)
            {
                await RemoveExp(exp, users);
                return;
            }
            using (var db = new DbService())
            {
                string userString = null;
                for (var i = 0; i < users.Length; i++)
                {
                    var user = users[i];
                    var userData = await db.GetOrCreateUserData(users[i]);
                    await _exp.AddExp(user, userData, exp, 0, db);
                    userString += $"{user.Mention}\n";
                }

                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Added {exp} to:\n" +
                                         $"{userString}");
            }
        }

        [Name("Remove Experience")]
        [Command("add remove")]
        [Description("Removes a certain amount of experience to a one or more users")]
        [Remarks("exp remove 100 @bob#0000")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task RemoveExp(int exp, params SocketGuildUser[] users)
        {
            if (exp <= 0) return;
            using (var db = new DbService())
            {
                string userString = null;
                for (var i = 0; i < users.Length; i++)
                {
                    var user = users[i];
                    var userData = await db.GetOrCreateUserData(users[i]);
                    await _exp.AddExp(user, userData, (exp * (-1)), 0, db);
                    userString += $"{user.Mention}\n";
                }

                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Removed {exp} from:\n" +
                                         $"{userString}");
            }
        }

        [Name("Add Exp Ignore Channel")]
        [Command("exp ignore add", "eia")]
        [Description("Adds one or more channels to ignore exp")]
        [Remarks("exp ignore add #general #bot etc")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task AddExpIgnoreChannel(params SocketTextChannel[] channels)
        {
            using (var db = new DbService())
            {
                var channeList = _exp.ServerChannelReduction.GetOrAdd(Context.Guild.Id, new HashSet<ulong>());
                var content = new StringBuilder();
                content.AppendLine("Channels added to exp ignore list:");
                for (var i = 0; i < channels.Length; i++)
                {
                    var x = channels[i];
                    if (!channeList.TryGetValue(x.Id, out _))
                    {
                        channeList.Add(x.Id);
                        var data = new LevelExpReduction
                        {
                            GuildId = Context.Guild.Id,
                            ChannelId = x.Id,
                            Channel = true,
                            Category = false
                        };
                        await db.LevelExpReductions.AddAsync(data);
                        content.AppendLine($"Added {x.Mention}");
                    }
                    else
                    {
                        content.AppendLine($"{x.Mention} is already added");
                    }
                }

                await db.SaveChangesAsync();
                await Context.ReplyAsync(content.ToString());
            }
        }

        [Name("Add Exp Ignore Category")]
        [Command("exp ignore add", "eia")]
        [Description("Adds one or more categories to ignore giving exp")]
        [Remarks("exp ignore add general information etc")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task AddExpIgnoreChannel(params SocketCategoryChannel[] category)
        {
            using (var db = new DbService())
            {
                var channeList = _exp.ServerCategoryReduction.GetOrAdd(Context.Guild.Id, new HashSet<ulong>());
                var content = new StringBuilder();
                content.AppendLine("Categories added to exp ignore list:");
                for (var i = 0; i < category.Length; i++)
                {
                    var x = category[i];
                    if (!channeList.TryGetValue(x.Id, out _))
                    {
                        channeList.Add(x.Id);
                        var data = new LevelExpReduction
                        {
                            GuildId = Context.Guild.Id,
                            ChannelId = x.Id,
                            Channel = false,
                            Category = true
                        };
                        await db.LevelExpReductions.AddAsync(data);
                        content.AppendLine($"Added {x.Name}");
                    }
                    else
                    {
                        content.AppendLine($"{x.Name} is already added");
                    }
                }

                await db.SaveChangesAsync();
                await Context.ReplyAsync(content.ToString());
            }
        }

        [Name("Remove Exp Ignore Channel")]
        [Command("exp ignore remove", "eir")]
        [Description("Removes one or more channels from ignore exp table")]
        [Remarks("exp ignore remove #general #bot etc")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task RemoveExpIgnoreChannel(params SocketTextChannel[] channels)
        {
            using (var db = new DbService())
            {
                var channeList = _exp.ServerChannelReduction.GetOrAdd(Context.Guild.Id, new HashSet<ulong>());
                var content = new StringBuilder();
                content.AppendLine("Channels removed from exp ignore list:");
                for (var i = 0; i < channels.Length; i++)
                {
                    var x = channels[i];
                    if (channeList.TryGetValue(x.Id, out _))
                    {
                        channeList.Remove(x.Id);
                        var data = await db.LevelExpReductions.FirstOrDefaultAsync(z =>
                            z.GuildId == Context.Guild.Id && z.ChannelId == x.Id);
                        db.LevelExpReductions.Remove(data);
                        content.AppendLine($"Removed {x.Mention}");
                    }
                    else
                    {
                        content.AppendLine($"Can't remove {x.Mention} as it's not added");
                    }
                }

                await db.SaveChangesAsync();
                await Context.ReplyAsync(content.ToString());
            }
        }

        [Name("Remove Exp Ignore Category")]
        [Command("exp ignore remove", "eir")]
        [Description("Removes one or more category from ignore exp table")]
        [Remarks("exp ignore remove general information etc")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task RemoveExpIgnoreChannel(params SocketCategoryChannel[] category)
        {
            using (var db = new DbService())
            {
                var channeList = _exp.ServerCategoryReduction.GetOrAdd(Context.Guild.Id, new HashSet<ulong>());
                var content = new StringBuilder();
                content.AppendLine("Categories removed from exp ignore list:");
                for (var i = 0; i < category.Length; i++)
                {
                    var x = category[i];
                    if (channeList.TryGetValue(x.Id, out _))
                    {
                        channeList.Remove(x.Id);
                        var data = await db.LevelExpReductions.FirstOrDefaultAsync(z =>
                            z.GuildId == Context.Guild.Id && z.ChannelId == x.Id);
                        db.LevelExpReductions.Remove(data);
                        content.AppendLine($"Removed {x.Name}");
                    }
                    else
                    {
                        content.AppendLine($"Can't remove {x.Name} as it's not added");
                    }
                }

                await db.SaveChangesAsync();
                await Context.ReplyAsync(content.ToString());
            }
        }

        [Name("Remove Exp Ignore Category")]
        [Command("exp ignore remove", "eir")]
        [Description("Removes one or more category from ignore exp table")]
        [Remarks("exp ignore remove general information etc")]
        [RequiredChannel]
        public async Task ExpIgnoreList()
        {
            var channels = _exp.ServerChannelReduction.GetOrAdd(Context.Guild.Id, new HashSet<ulong>());
            var categories = _exp.ServerCategoryReduction.GetOrAdd(Context.Guild.Id, new HashSet<ulong>());
            var result = new List<string>();
            if (channels.Count == 0 && categories.Count == 0)
            {
                result.Add("No channels");
            }

            if (channels.Count > 0)
            {
                foreach (var x in channels)
                {
                    var channel = Context.Guild.GetTextChannel(x);
                    if (channel == null) continue;
                    result.Add($"Channel: {channel.Mention}");
                }
            }

            if (categories.Count > 0)
            {
                foreach (var x in categories)
                {
                    var category = Context.Guild.GetCategoryChannel(x);
                    if (category == null) continue;
                    result.Add($"Category: {category.Name}");
                }
            }

            await PagedReplyAsync(result.PaginateBuilder(Context.Guild, "Experience Channel Ignore", null));
        }
    }
}