using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Extensions.Embed;

namespace Hanekawa.Modules.Club
{
    [Group("club settings")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    [RequireContext(ContextType.Guild)]
    public class ClubAdmin : InteractiveBase
    {
        [Command("advertisement", RunMode = RunMode.Async)]
        [Summary("Sets channel where club advertisements will be posted. \nLeave empty to disable")]
        public async Task ClubSetAdvertismentChannel(ITextChannel channel = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
                if (channel == null)
                {
                    cfg.ClubAdvertisementChannel = null;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Disabled Advertisement creation of clubs.",
                        Color.Green.RawValue);
                    return;
                }

                if (cfg.ClubAdvertisementChannel.HasValue && cfg.ClubAdvertisementChannel.Value == channel.Id)
                {
                    await Context.ReplyAsync($"Advertisement channel has already been set to {channel.Mention}",
                        Color.Red.RawValue);
                }
                else
                {
                    cfg.ClubAdvertisementChannel = channel.Id;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync($"Advertisement channel set has been been set to {channel.Mention}",
                        Color.Green.RawValue);
                }
            }
        }

        [Command("category", RunMode = RunMode.Async)]
        [Summary("Sets location in where club channels will be created. \nLeave empty to disable")]
        public async Task ClubSetCategory(ICategoryChannel category = null)
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
                if (cfg.ClubChannelCategory.HasValue && cfg.ClubChannelCategory.Value == category.Id)
                {
                    await Context.ReplyAsync($"Club category channel has already been set to {category.Name}",
                        Color.Red.RawValue);
                    return;
                }

                if (category == null)
                {
                    cfg.ClubChannelCategory = null;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync("Disabled club channel creation",
                        Color.Green.RawValue);
                }
                else
                {
                    cfg.ClubChannelCategory = category.Id;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync($"Club category channel set has been been set to {category.Name}",
                        Color.Green.RawValue);
                }
            }
        }

        [Command("level", RunMode = RunMode.Async)]
        [Summary("Sets level requirement for people to create a club")]
        public async Task ClubSetLevelRequirement(int level)
        {
            if (level <= 0) return;
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
                cfg.ClubChannelRequiredLevel = level;
                await db.SaveChangesAsync();
                await Context.ReplyAsync(
                    $"Set required amount of club members above the level limit to create a channel to {level}",
                    Color.Green.RawValue);
            }
        }

        [Command("channel amount", RunMode = RunMode.Async)]
        [Summary("Sets amount required thats above the level requirement(club create) to create a channel")]
        public async Task ClubSetAmountRequirement(int amount)
        {
            if (amount <= 0) return;
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
                cfg.ClubChannelRequiredAmount = amount;
                await db.SaveChangesAsync();
                await Context.ReplyAsync(
                    $"Set required amount of club members above the level limit to create a channel to {amount}",
                    Color.Green.RawValue);
            }
        }

        [Command("Auto prune")]
        [Alias("autoprune", "prune")]
        [Summary("Automatically prune inactive clubs by their member count")]
        public async Task ClubAutoPruneToggle()
        {
            using (var db = new DbService())
            {
                var cfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
                if (cfg.ClubAutoPrune)
                {
                    cfg.ClubAutoPrune = false;
                    await Context.ReplyAsync("Disabled automatic deletion of low member count clubs with a channel.",
                        Color.Green.RawValue);
                }
                else
                {
                    cfg.ClubAutoPrune = true;
                    await Context.ReplyAsync("Enabled automatic deletion of low member count clubs with a channel.",
                        Color.Green.RawValue);
                }

                await db.SaveChangesAsync();
            }
        }
    }
}