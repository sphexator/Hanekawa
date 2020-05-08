using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Modules.Club
{
    public partial class Club
    {
        [Name("Club Setting Advertisement")]
        [Command("ca")]
        [Description("Sets channel where club advertisements will be posted. \nLeave empty to disable")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task ClubSetAdvertisementChannel(CachedTextChannel channel = null)
        {
            using var scope = Context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateClubConfigAsync(Context.Guild);
            if (channel == null)
            {
                cfg.AdvertisementChannel = null;
                await db.SaveChangesAsync();
                await Context.ReplyAsync("Disabled Advertisement creation of clubs.",
                    Color.Green);
                return;
            }

            if (cfg.AdvertisementChannel.HasValue && cfg.AdvertisementChannel.Value == channel.Id.RawValue)
            {
                await Context.ReplyAsync($"Advertisement channel has already been set to {channel.Mention}",
                    Color.Red);
            }
            else
            {
                cfg.AdvertisementChannel = channel.Id.RawValue;
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Advertisement channel set has been been set to {channel.Mention}",
                    Color.Green);
            }
        }

        [Name("Club Setting Category")]
        [Command("Clubcategory", "ccat")]
        [Description("Sets location in where club channels will be created. \nLeave empty to disable")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task ClubSetCategory(CachedCategoryChannel category = null)
        {
            using var scope = Context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateClubConfigAsync(Context.Guild);
            if (cfg.ChannelCategory.HasValue && cfg.ChannelCategory.Value == category.Id.RawValue)
            {
                await Context.ReplyAsync($"Club category channel has already been set to {category.Name}",
                    Color.Red);
                return;
            }

            if (category == null)
            {
                cfg.ChannelCategory = null;
                await db.SaveChangesAsync();
                await Context.ReplyAsync("Disabled club channel creation",
                    Color.Green);
            }
            else
            {
                cfg.ChannelCategory = category.Id.RawValue;
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Club category channel set has been been set to {category.Name}",
                    Color.Green);
            }
        }

        [Name("Club Setting Level")]
        [Command("Clublevel", "cl")]
        [Description("Sets level requirement for people to create a club")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task ClubSetLevelRequirement(int level)
        {
            if (level <= 0) return;
            using var scope = Context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateClubConfigAsync(Context.Guild);
            cfg.ChannelRequiredLevel = level;
            await db.SaveChangesAsync();
            await Context.ReplyAsync(
                $"Set required level limit to create a club to {level}",
                Color.Green);
        }

        [Name("Club Setting Channel Amount")]
        [Command("cca")]
        [Description("Sets amount required that's above the level requirement to create a channel")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task ClubSetAmountRequirement(int amount)
        {
            if (amount <= 0) return;
            using var scope = Context.ServiceProvider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateClubConfigAsync(Context.Guild);
            cfg.ChannelRequiredAmount = amount;
            await db.SaveChangesAsync();
            await Context.ReplyAsync(
                $"Set required amount of club members above the level limit to create a channel to {amount}",
                Color.Green);
        }
    }
}