using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Bot.Commands.Preconditions;
using Hanekawa.Bot.Service.Mvp;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Entities.Color;
using Hanekawa.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Commands.Modules.Setting
{
    [Name("MVP")]
    [Description("Reward most active users of the week")]
    [RequirePremium]
    public class Mvp : HanekawaCommandModule
    {
        [Name("Opt Out")]
        [Command("Optout")]
        [Description("Opts out from being counted toward the weekly MVP")]
        [RequiredChannel]
        public async Task<DiscordCommandResult> OptOutAsync()
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var userData = await db.GetOrCreateUserData(Context.Author);
            if (userData.MvpOptOut)
            {
                userData.MvpOptOut = false;
                await db.SaveChangesAsync();
                return Reply("Re-enabled MVP counting!", HanaBaseColor.Ok());
            }

            userData.MvpOptOut = true;
            await db.SaveChangesAsync();
            return Reply("Opted out of MVP counting!", HanaBaseColor.Ok());
        }

        [Name("MVP Settings")]
        [Description("Configure the MVP module")]
        [Group("Mvp")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public class MvpAdmin : Mvp
        {
            [Name("Set Channel")]
            [Command("channel")]
            [Description("Sets the channel new users gets announced")]
            public async Task<DiscordCommandResult> SetAnnouncementChannelAsync(ITextChannel channel = null)
            {
                await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateGuildConfigAsync(Context.Guild);
                if (channel == null)
                {
                    await db.SaveChangesAsync();
                    cfg.MvpChannel = null;
                    return Reply("Disabled MVP Announcement!", HanaBaseColor.Ok());
                }

                await db.SaveChangesAsync();
                cfg.MvpChannel = channel.Id;
                return Reply($"Set MVP Announcement channel to {channel.Mention} !", HanaBaseColor.Ok());
            }

            [Name("Set Role")]
            [Command("role")]
            [Description("Sets the role that's rewarded to MVP users")]
            public async Task<DiscordCommandResult> SetMvpRoleAsync(IRole role = null)
            {
                await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateMvpConfigAsync(Context.Guild);
                if (role == null)
                {
                    cfg.RoleId = null;
                    await db.SaveChangesAsync();
                    return Reply("Disabled MVP!", HanaBaseColor.Ok());
                }

                cfg.RoleId = role.Id;
                await db.SaveChangesAsync();
                return Reply($"Set MVP role to {role.Name} !", HanaBaseColor.Ok());
            }

            [Name("Exp Reward")]
            [Command("exp")]
            [Description("Reward MVP users with experience")]
            public async Task<DiscordCommandResult> SetMvpExpRewardAsync(int exp = 0)
            {
                if (exp < 0) exp = 0;
                await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateMvpConfigAsync(Context.GuildId);
                cfg.ExpReward = exp;
                return Reply($"Set exp reward to {exp}!", HanaBaseColor.Ok());
            }

            [Name("Credit Reward")]
            [Command("credit")]
            [Description("Reward MVP users with credit")]
            public async Task<DiscordCommandResult> SetMvpCreditRewardAsync(int credit = 0)
            {
                if (credit < 0) credit = 0;
                await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateMvpConfigAsync(Context.GuildId);
                var currencyCfg = await db.GetOrCreateCurrencyConfigAsync(Context.GuildId);
                cfg.CreditReward = credit;
                return Reply($"Set {currencyCfg.CurrencyName} reward to {currencyCfg.ToCurrencyFormat(credit)}!",
                    HanaBaseColor.Ok());
            }

            [Name("Special Credit Reward")]
            [Command("scredit")]
            [Description("Reward MVP users with special credit")]
            public async Task<DiscordCommandResult> SetMvpSpecialCreditRewardAsync(int specialCredit = 0)
            {
                if (specialCredit < 0) specialCredit = 0;
                await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateMvpConfigAsync(Context.GuildId);
                var currencyCfg = await db.GetOrCreateCurrencyConfigAsync(Context.GuildId);
                cfg.SpecialCreditReward = specialCredit;
                return Reply(
                    $"Set {currencyCfg.SpecialCurrencyName} reward to {currencyCfg.ToCurrencyFormat(specialCredit, true)}!",
                    HanaBaseColor.Ok());
            }

            [Name("MVP Reward Day")]
            [Command("day")]
            [Description("Sets which day the MVP rewards is gonna be handed out")]
            public async Task<DiscordCommandResult> SetMvpDayAsync(DayOfWeek? day = null)
            {
                await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateMvpConfigAsync(Context.Guild);
                if (!day.HasValue)
                {
                    cfg.Disabled = true;
                    await db.SaveChangesAsync();
                    return Reply("Disabled MVP service!", HanaBaseColor.Ok());
                }

                if (day == cfg.Day) return Reply($"Mvp reward is already set to {day.Value}!", HanaBaseColor.Bad());
                cfg.Disabled = false;
                cfg.Day = day.Value;
                await db.SaveChangesAsync();
                return Reply($"Set MVP reward day to {day.Value}!", HanaBaseColor.Ok());
            }

            [Name("Force Mvp Reward")]
            [Command("force")]
            [Description("Force MVP rewards in-case it doesn't go out")]
            [RequireAuthorGuildPermissions(Permission.Administrator)]
            public async Task<DiscordCommandResult> ForceMvpAsync()
            {
                await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
                var guildConfig = await db.GetOrCreateGuildConfigAsync(Context.Guild);
                await Reply("Executing MVP rewards...");
                await Context.Services.GetRequiredService<MvpService>().RewardAsync(guildConfig, db, true);
                return Reply("Rewarded MVP users and reset MVP counter", HanaBaseColor.Ok());
            }
        }
    }
}