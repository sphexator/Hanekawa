﻿using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Shared.Command;
using Microsoft.EntityFrameworkCore;
using Qmmands;
using ChannelType = Hanekawa.Shared.ChannelType;

namespace Hanekawa.Bot.Modules.Level
{
    public partial class Level
    {
        [Name("Add Experience")]
        [Command("addexp")]
        [Description("Give a certain amount of experience to a one or more users")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task AddExp(int exp, params CachedMember[] users)
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
                    await _exp.AddExpAsync(user, userData, exp, 0, db);
                    userString += $"{user.Mention ?? "Couldn't find user"}\n";
                }

                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Added {exp} to:\n" +
                                         $"{userString}");
            }
        }

        [Name("Remove Experience")]
        [Command("remexp")]
        [Description("Removes a certain amount of experience to a one or more users")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task RemoveExp(int exp, params CachedMember[] users)
        {
            if (exp <= 0) return;
            using (var db = new DbService())
            {
                string userString = null;
                for (var i = 0; i < users.Length; i++)
                {
                    var user = users[i];
                    var userData = await db.GetOrCreateUserData(users[i]);
                    await _exp.AddExpAsync(user, userData, exp * -1, 0, db);
                    userString += $"{user.Mention}\n";
                }

                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Removed {exp} from:\n" +
                                         $"{userString}");
            }
        }

        [Name("Add Exp Ignore Channel")]
        [Command("eia")]
        [Description("Adds one or more channels to ignore exp")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task AddExpIgnoreChannel(params CachedTextChannel[] channels)
        {
            using (var db = new DbService())
            {
                var channeList = _exp.ServerTextChanReduction.GetOrAdd(Context.Guild.Id, new HashSet<ulong>());
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
                            ChannelType = ChannelType.Text
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

        [Name("Add Exp Ignore Channel")]
        [Command("eia")]
        [Description("Adds one or more channels to ignore exp")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task AddExpIgnoreChannel(params CachedVoiceChannel[] channels)
        {
            using (var db = new DbService())
            {
                var channeList = _exp.ServerVoiceChanReduction.GetOrAdd(Context.Guild.Id, new HashSet<ulong>());
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
                            ChannelType = ChannelType.Voice
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

        [Name("Add Exp Ignore Category")]
        [Command("eia")]
        [Description("Adds one or more categories to ignore giving exp")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task AddExpIgnoreChannel(params CachedCategoryChannel[] category)
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
                            ChannelType = ChannelType.Category
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
        [Command("eir")]
        [Description("Removes one or more channels from ignore exp table")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task RemoveExpIgnoreChannel(params CachedTextChannel[] channels)
        {
            using (var db = new DbService())
            {
                var channeList = _exp.ServerTextChanReduction.GetOrAdd(Context.Guild.Id, new HashSet<ulong>());
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
        [Command("eir")]
        [Description("Removes one or more category from ignore exp table")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task RemoveExpIgnoreChannel(params CachedCategoryChannel[] category)
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
        [Command("eir")]
        [Description("Removes one or more category from ignore exp table")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        [RequiredChannel]
        public async Task ExpIgnoreList()
        {
            var channels = _exp.ServerTextChanReduction.GetOrAdd(Context.Guild.Id, new HashSet<ulong>());
            var categories = _exp.ServerCategoryReduction.GetOrAdd(Context.Guild.Id, new HashSet<ulong>());
            var result = new StringBuilder();
            var pages = new List<Page>();
            if (channels.Count == 0 && categories.Count == 0) result.AppendLine("No channels");
            var count = 0;
            if (channels.Count > 0)
                foreach (var x in channels)
                {
                    var channel = Context.Guild.GetTextChannel(x);
                    if (channel == null) continue;
                    if (count > 5)
                    {
                        pages.Add(new Page(result.ToString()));
                        result.Clear();
                        count = 0;
                    }
                    result.AppendLine($"Channel: {channel.Mention}");
                    count++;
                }

            if (categories.Count > 0)
                foreach (var x in categories)
                {
                    var category = Context.Guild.GetCategoryChannel(x);
                    if (category == null) continue;
                    if (count > 5)
                    {
                        pages.Add(new Page(result.ToString()));
                        result.Clear();
                        count = 0;
                    }
                    result.AppendLine($"Category: {category.Name}");
                    count++;
                }

            await Context.Bot.GetInteractivity().StartMenuAsync(Context.Channel,
                new PagedMenu(Context.Member.Id, new DefaultPageProvider(pages)));
        }
    }
}