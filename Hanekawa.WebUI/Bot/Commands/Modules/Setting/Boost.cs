using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Rest;
using Hanekawa.Infrastructure;
using Hanekawa.Infrastructure.Extensions;
using Hanekawa.Entities;
using Hanekawa.Entities.Color;
using Hanekawa.Entities.Config.Guild;
using Hanekawa.WebUI.Bot.Service.Experience;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.WebUI.Bot.Commands.Modules.Setting
{
    [Name("Boost")]
    [Description("Configure boost rewards")]
    [Group("Boost")]
    [RequireAuthorGuildPermissions(Permission.ManageGuild)]
    public class Boost : HanekawaCommandModule
    {
        [Name("Set Exp Reward")]
        [Command("exp")]
        [Description("Rewards the user a certain amount of exp for boosting the server")]
        public async Task<DiscordCommandResult> SetBoostExpAsync(int exp = 0)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateEntityAsync<BoostConfig>(Context.GuildId);
            cfg.ExpGain = exp;
            await db.SaveChangesAsync();
            return Reply($"Set experience boost reward to {exp}!", HanaBaseColor.Ok());
        }

        [Name("Set Credit Reward")]
        [Command("credit")]
        [Description("Rewards the user a certain amount of credit for boosting the server")]
        public async Task<DiscordCommandResult> SetBoostCreditAsync(int credit = 0)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateEntityAsync<BoostConfig>(Context.GuildId);
            cfg.CreditGain = credit;
            await db.SaveChangesAsync();
            return Reply($"Set credit boost reward to {credit}!", HanaBaseColor.Ok());
        }

        [Name("Set Special Credit Reward")]
        [Command("scredit")]
        [Description("Rewards the user a certain amount of special credit for boosting the server")]
        public async Task<DiscordCommandResult> SetBoostSpecialCreditAsync(int specialCredit = 0)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateEntityAsync<BoostConfig>(Context.GuildId);
            cfg.SpecialCreditGain = specialCredit;
            await db.SaveChangesAsync();
            return Reply($"Set special credit boost reward to {specialCredit}!", HanaBaseColor.Ok());
        }

        [Name("Set Exp Multiplier")]
        [Command("expmulti")]
        [Description("Sets a experience multiplier for everyone boosting to get rewarded a little extra experience every time they earn exp")]
        public async Task<DiscordCommandResult> SetBoostExpMultiplier(double multiplier = 1)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateEntityAsync<LevelConfig>(Context.GuildId);
            cfg.BoostExpMultiplier = multiplier;
            await db.SaveChangesAsync();
            return Reply($"Set experience multiplier for boosting people to {multiplier}!", HanaBaseColor.Ok());
        }

        [Name("Announcement Channel")]
        [Command("channel")]
        [Description("Sets a channel to announce people that boosted the server")]
        public async Task<DiscordCommandResult> SetAnnouncementChannelAsync(CachedTextChannel channel = null)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateEntityAsync<BoostConfig>(Context.GuildId);
            if (channel == null)
            {
                cfg.ChannelId = null;
                await db.SaveChangesAsync();
                return Reply($"Disabled boost announcements!", HanaBaseColor.Ok());
            }

            cfg.ChannelId = channel.Id;
            await db.SaveChangesAsync();
            return Reply($"Set boost announcement channel to {channel.Mention}!", HanaBaseColor.Ok());
        }

        [Name("Force Reward")]
        [Command("force")]
        [Description("Force reward any existing users boosting the server")]
        [RequireAuthorGuildPermissions(Permission.Administrator)]
        public async Task<DiscordCommandResult> ForceBoostReward(IMember user = null)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateEntityAsync<BoostConfig>(Context.GuildId);
            var expService = Context.Services.GetRequiredService<ExpService>();
            global::Hanekawa.Entities.Account.Account userData;
            if (user != null)
            {
                userData = await db.GetOrCreateEntityAsync<global::Hanekawa.Entities.Account.Account>(Context.GuildId, user.Id);
                await expService.AddExpAsync(user, userData, cfg.ExpGain, cfg.CreditGain, db, ExpSource.Other);
                userData.CreditSpecial += cfg.SpecialCreditGain;
                await db.SaveChangesAsync();
                return Reply($"Added boost rewards to {user.Mention}!\n" +
                                 $"+ {cfg.ExpGain} exp\n" +
                                 $"+ {cfg.CreditGain} credit\n" +
                                 $"+ {cfg.SpecialCreditGain} special credit", HanaBaseColor.Ok());
            }

            await Context.Channel.TriggerTypingAsync();
            var users = Context.Guild.Members.Where(x => x.Value.BoostedAt.HasValue).ToList();
            foreach (var x in users)
            {
                var (_, cachedMember) = x;
                userData = await db.GetOrCreateEntityAsync<global::Hanekawa.Entities.Account.Account>(Context.GuildId,
                    cachedMember.Id);
                await expService.AddExpAsync(cachedMember, userData, cfg.ExpGain, cfg.CreditGain, db, ExpSource.Other);
                userData.CreditSpecial += cfg.SpecialCreditGain;
                await db.SaveChangesAsync();
            }
            return Reply($"Added boost rewards to {users.Count} users!\n" +
                             $"+ {cfg.ExpGain} exp\n" +
                             $"+ {cfg.CreditGain} credit\n" +
                             $"+ {cfg.SpecialCreditGain} special credit", HanaBaseColor.Ok());
        }
    }
}