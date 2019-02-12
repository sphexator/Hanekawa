using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Modules.Club.Handler;
using Microsoft.EntityFrameworkCore;
using Quartz.Util;
using System.Threading.Tasks;

namespace Hanekawa.Modules.Club
{
    [Name("Club settings")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    [RequireContext(ContextType.Guild)]
    public class ClubAdmin : InteractiveBase
    {
        private readonly Advertise _advertise;

        public ClubAdmin(Advertise advertise) => _advertise = advertise;

        [Name("Club setting advertisement")]
        [Command("Club set advertisement", RunMode = RunMode.Async)]
        [Alias("csa")]
        [Summary("Sets channel where club advertisements will be posted. \nLeave empty to disable")]
        [Remarks("h.csa #general")]
        public async Task ClubSetAdvertisementChannel(ITextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateClubConfigAsync(Context.Guild);
                if (channel == null)
                {
                    cfg.AdvertisementChannel = null;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Disabled Advertisement creation of clubs.",
                        Color.Green.RawValue);
                    return;
                }

                if (cfg.AdvertisementChannel.HasValue && cfg.AdvertisementChannel.Value == channel.Id)
                {
                    await Context.ReplyAsync($"Advertisement channel has already been set to {channel.Mention}",
                        Color.Red.RawValue);
                }
                else
                {
                    cfg.AdvertisementChannel = channel.Id;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync($"Advertisement channel set has been been set to {channel.Mention}",
                        Color.Green.RawValue);
                }
            }
        }

        [Name("Club setting category")]
        [Command("Club set category", RunMode = RunMode.Async)]
        [Alias("clubscategory", "cscat")]
        [Summary("Sets location in where club channels will be created. \nLeave empty to disable")]
        [Remarks("h.cscat general")]
        public async Task ClubSetCategory(ICategoryChannel category = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateClubConfigAsync(Context.Guild);
                if (cfg.ChannelCategory.HasValue && cfg.ChannelCategory.Value == category.Id)
                {
                    await Context.ReplyAsync($"Club category channel has already been set to {category.Name}",
                        Color.Red.RawValue);
                    return;
                }

                if (category == null)
                {
                    cfg.ChannelCategory = null;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Disabled club channel creation",
                        Color.Green.RawValue);
                }
                else
                {
                    cfg.ChannelCategory = category.Id;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync($"Club category channel set has been been set to {category.Name}",
                        Color.Green.RawValue);
                }
            }
        }

        [Name("Club setting level")]
        [Command("Club set level", RunMode = RunMode.Async)]
        [Alias("csl")]
        [Summary("Sets level requirement for people to create a club")]
        [Remarks("h.csl 10")]
        public async Task ClubSetLevelRequirement(int level)
        {
            if (level <= 0) return;
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateClubConfigAsync(Context.Guild);
                cfg.ChannelRequiredLevel = level;
                await db.SaveChangesAsync();
                await Context.ReplyAsync(
                    $"Set required amount of club members above the level limit to create a channel to {level}",
                    Color.Green.RawValue);
            }
        }

        [Name("Club setting channel amount")]
        [Command("club set channel amount", RunMode = RunMode.Async)]
        [Alias("csca")]
        [Summary("Sets amount required that's above the level requirement to create a channel")]
        [Remarks("h.csca 2")]
        public async Task ClubSetAmountRequirement(int amount)
        {
            if (amount <= 0) return;
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateClubConfigAsync(Context.Guild);
                cfg.ChannelRequiredAmount = amount;
                await db.SaveChangesAsync();
                await Context.ReplyAsync(
                    $"Set required amount of club members above the level limit to create a channel to {amount}",
                    Color.Green.RawValue);
            }
        }

        [Name("Rename club")]
        [Command("club set name", RunMode = RunMode.Async)]
        [Alias("csn")]
        [Summary("Force changes a name of a club")]
        [Remarks("h.csn 15 Change name")]
        public async Task ClubForceRename(int clubId, [Remainder] string name)
        {
            using (var db = new DbService())
            {
                var club = await db.ClubInfos.FirstOrDefaultAsync(x => x.Id == clubId && x.GuildId == Context.Guild.Id);
                if (club == null)
                {
                    await Context.ReplyAsync("There's no club with that ID in this guild");
                    return;
                }

                if (name.IsNullOrWhiteSpace())
                {
                    await Context.ReplyAsync("Please provide a proper name");
                    return;
                }

                var cfg = await db.GetOrCreateClubConfigAsync(Context.Guild);
                club.Name = name;
                await db.SaveChangesAsync();
                if (club.AdMessage.HasValue && cfg.AdvertisementChannel.HasValue)
                {
                    var msg = await Context.Guild.GetTextChannel(cfg.AdvertisementChannel.Value).GetMessageAsync(club.AdMessage.Value) as IUserMessage;
                    await _advertise.UpdatePostNameAsync(msg, name);
                }
                if (club.Role.HasValue)
                {
                    var role = Context.Guild.GetRole(club.Role.Value);
                    await role.ModifyAsync(x => x.Name = name);
                }

                if (club.Channel.HasValue)
                {
                    var channel = Context.Guild.GetTextChannel(club.Channel.Value);
                    await channel.ModifyAsync(x => x.Name = name);
                }
            }
        }

        [Name("Change club icon")]
        [Command("club set icon", RunMode = RunMode.Async)]
        [Alias("csicon")]
        [Summary("Force changes icon of a club")]
        [Remarks("h.csn 15 https://i.imgur.com/p3Xxvij.png")]
        public async Task ClubForceReIcon(int clubId, [Remainder] string icon)
        {
            using (var db = new DbService())
            {
                var club = await db.ClubInfos.FirstOrDefaultAsync(x => x.Id == clubId && x.GuildId == Context.Guild.Id);
                if (club == null)
                {
                    await Context.ReplyAsync("There's no club with that ID in this guild");
                    return;
                }

                var cfg = await db.GetOrCreateClubConfigAsync(Context.Guild);
                club.IconUrl = icon;
                await db.SaveChangesAsync();
                if (club.AdMessage.HasValue && cfg.AdvertisementChannel.HasValue)
                {
                    var msg = await Context.Guild.GetTextChannel(cfg.AdvertisementChannel.Value).GetMessageAsync(club.AdMessage.Value) as IUserMessage;
                    await _advertise.UpdatePostIconAsync(msg, icon);
                }
            }
        }

        [Name("Change club image")]
        [Command("club set image", RunMode = RunMode.Async)]
        [Alias("csimage")]
        [Summary("Force changes image of a club")]
        [Remarks("h.csn 15 https://i.imgur.com/p3Xxvij.png")]
        public async Task ClubForceReImage(int clubId, [Remainder] string image)
        {
            using (var db = new DbService())
            {
                var club = await db.ClubInfos.FirstOrDefaultAsync(x => x.Id == clubId && x.GuildId == Context.Guild.Id);
                if (club == null)
                {
                    await Context.ReplyAsync("There's no club with that ID in this guild");
                    return;
                }

                var cfg = await db.GetOrCreateClubConfigAsync(Context.Guild);
                club.ImageUrl = image;
                await db.SaveChangesAsync();
                if (club.AdMessage.HasValue && cfg.AdvertisementChannel.HasValue)
                {
                    var msg = await Context.Guild.GetTextChannel(cfg.AdvertisementChannel.Value).GetMessageAsync(club.AdMessage.Value) as IUserMessage;
                    await _advertise.UpdatePostImageAsync(msg, image);
                }
            }
        }

        [Name("Change club description")]
        [Command("club set description", RunMode = RunMode.Async)]
        [Alias("csd")]
        [Summary("Force changes a name of a club")]
        [Remarks("h.csn 15 Change description")]
        public async Task ClubForceReDescription(int clubId, [Remainder] string desc)
        {
            using (var db = new DbService())
            {
                var club = await db.ClubInfos.FirstOrDefaultAsync(x => x.Id == clubId && x.GuildId == Context.Guild.Id);
                if (club == null)
                {
                    await Context.ReplyAsync("There's no club with that ID in this guild");
                    return;
                }

                if (desc.IsNullOrWhiteSpace()) desc = "N/A";

                var cfg = await db.GetOrCreateClubConfigAsync(Context.Guild);
                club.Description = desc;
                await db.SaveChangesAsync();
                if (club.AdMessage.HasValue && cfg.AdvertisementChannel.HasValue)
                {
                    var msg = await Context.Guild.GetTextChannel(cfg.AdvertisementChannel.Value).GetMessageAsync(club.AdMessage.Value) as IUserMessage;
                    await _advertise.UpdatePostDescriptionAsync(msg, desc);
                }
            }
        }

        [Name("Club role toggle")]
        [Command("club toggle role")]
        [Alias("ctr")]
        [Summary("Toggles the use of creating roles for club or channel permission. Auto to channel when above 50 roles")]
        [Remarks("h.ctr")]
        public async Task ToggleClubRole()
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateClubConfigAsync(Context.Guild);
                if (cfg.RoleEnabled)
                {
                    cfg.RoleEnabled = false;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Disabled creation of roles for clubs.\n" +
                                             "Now using channel permissions to add users to their designated channel");
                }
                else
                {
                    cfg.RoleEnabled = true;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Enabled creation of roles for clubs.\n" +
                                             "Now using their designated role to add users to their channel");
                }
            }
        }
    }
}