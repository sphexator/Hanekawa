using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions.Embed;
using Qmmands;

namespace Hanekawa.Bot.Modules.Club
{
    public partial class Club
    {
        [Name("Club Setting Advertisement")]
        [Command("Club set advertisement", "csa")]
        [Description("Sets channel where club advertisements will be posted. \nLeave empty to disable")]
        [Remarks("csa #general")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task ClubSetAdvertisementChannel(SocketTextChannel channel = null)
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

        [Name("Club Setting Category")]
        [Command("Club set category", "clubscategory", "cscat")]
        [Description("Sets location in where club channels will be created. \nLeave empty to disable")]
        [Remarks("cscat general")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task ClubSetCategory(SocketCategoryChannel category = null)
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

        [Name("Club Setting Level")]
        [Command("Club set level", "csl")]
        [Description("Sets level requirement for people to create a club")]
        [Remarks("csl 10")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task ClubSetLevelRequirement(int level)
        {
            if (level <= 0) return;
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateClubConfigAsync(Context.Guild);
                cfg.ChannelRequiredLevel = level;
                await db.SaveChangesAsync();
                await Context.ReplyAsync(
                    $"Set required level limit to create a club to {level}",
                    Color.Green.RawValue);
            }
        }

        [Name("Club Setting Channel Amount")]
        [Command("club set channel amount", "csca")]
        [Description("Sets amount required that's above the level requirement to create a channel")]
        [Remarks("csca 2")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
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
    }
}
