using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Bot.Services.Experience;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Shared.Command;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Modules.Settings
{
    public class Boost : HanekawaCommandModule
    {
        [Name("Set Exp Reward")]
        [Command("boostexp")]
        [Description("Rewards the user a certain amount of exp for boosting the server")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task SetBoostExpAsync(int exp = 0)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateBoostConfigAsync(Context.Guild);
            cfg.ExpGain = exp;
            await db.SaveChangesAsync();
            await ReplyAsync($"Set experience boost reward to {exp}!", Color.Green);
        }

        [Name("Set Credit Reward")]
        [Command("boostcredit")]
        [Description("Rewards the user a certain amount of credit for boosting the server")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task SetBoostCreditAsync(int credit = 0)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateBoostConfigAsync(Context.Guild);
            cfg.CreditGain = credit;
            await db.SaveChangesAsync();
            await ReplyAsync($"Set credit boost reward to {credit}!", Color.Green);
        }

        [Name("Set Special Credit Reward")]
        [Command("boostscredit")]
        [Description("Rewards the user a certain amount of special credit for boosting the server")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task SetBoostSpecialCreditAsync(int specialCredit = 0)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateBoostConfigAsync(Context.Guild);
            cfg.SpecialCreditGain = specialCredit;
            await db.SaveChangesAsync();
            await ReplyAsync($"Set special credit boost reward to {specialCredit}!", Color.Green);
        }

        [Name("Set Exp Multiplier")]
        [Command("boostexpmulti")]
        [Description("Sets a experience multiplier for everyone boosting to get rewarded a little extra experience every time they earn exp")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task SetBoostExpMultiplier(double multiplier = 1)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateLevelConfigAsync(Context.Guild);
            cfg.BoostExpMultiplier = multiplier;
            await db.SaveChangesAsync();
            await ReplyAsync($"Set experience multiplier for boosting people to {multiplier}!", Color.Green);
        }

        [Name("Announcement Channel")]
        [Command("boostchannel")]
        [Description("Sets a channel to announce people that boosted the server")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task SetAnnouncementChannelAsync(CachedTextChannel channel = null)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateBoostConfigAsync(Context.Guild);
            if (channel == null)
            {
                cfg.ChannelId = null;
                await db.SaveChangesAsync();
                await ReplyAsync($"Disabled boost announcements!", Color.Green);
                return;
            }

            cfg.ChannelId = channel.Id.RawValue;
            await db.SaveChangesAsync();
            await ReplyAsync($"Set boost announcement channel to {channel.Mention}!", Color.Green);
        }

        [Name("Force Reward")]
        [Command("boostfreward")]
        [Description("Force reward any existing users boosting the server")]
        [RequireMemberGuildPermissions(Permission.Administrator)]
        public async Task ForceBoostReward(CachedMember user = null)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateBoostConfigAsync(Context.Guild);
            var expService = Context.ServiceProvider.GetRequiredService<ExpService>();
            Database.Tables.Account.Account userData;
            if (user != null)
            {
                userData = await db.GetOrCreateUserData(user);
                await expService.AddExpAsync(user, userData, cfg.ExpGain, cfg.CreditGain, db);
                userData.CreditSpecial += cfg.SpecialCreditGain;
                await db.SaveChangesAsync();
                await ReplyAsync($"Added boost rewards to {user.Mention}!\n" +
                                 $"+ {cfg.ExpGain} exp\n" +
                                 $"+ {cfg.CreditGain} credit\n" +
                                 $"+ {cfg.SpecialCreditGain} special credit", Color.Green);
                return;
            }

            await Context.Channel.TriggerTypingAsync();
            var users = Context.Guild.Members.Where(x => x.Value.IsBoosting).ToList();
            for (var i = 0; i < users.Count; i++)
            {
                var (_, cachedMember) = users[i];
                userData = await db.GetOrCreateUserData(cachedMember);
                await expService.AddExpAsync(cachedMember, userData, cfg.ExpGain, cfg.CreditGain, db);
                userData.CreditSpecial += cfg.SpecialCreditGain;
                await db.SaveChangesAsync();
            }
            await ReplyAsync($"Added boost rewards to {users.Count} users!\n" +
                             $"+ {cfg.ExpGain} exp\n" +
                             $"+ {cfg.CreditGain} credit\n" +
                             $"+ {cfg.SpecialCreditGain} special credit", Color.Green);
        }
    }
}