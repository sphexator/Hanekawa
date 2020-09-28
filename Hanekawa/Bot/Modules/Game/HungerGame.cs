using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Bot.Services.Game.HungerGames;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Shared.Command;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using Quartz.Util;

namespace Hanekawa.Bot.Modules.Game
{
    [Name("Hunger Games")]
    [RequirePremium]
    public class HungerGame : HanekawaCommandModule
    {
        private readonly HungerGameService _service;

        public HungerGame(HungerGameService service) => _service = service;

        [Name("Start")]
        [Command("hgstart")]
        [Description("Forcefully starts a hunger game")]
        [RequireMemberGuildPermissions(Permission.Administrator)]
        public async Task StartAsync()
        {
            await _service.Execute(null);
        }

        [Name("Set Sign-up emote")]
        [Command("hgemote")]
        [Description("Sets the emote used to sign-up for hunger game")]
        [RequireMemberGuildPermissions(Permission.Administrator)]
        public async Task SetSignUpEmoteAsync(LocalCustomEmoji emote)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateHungerGameStatus(Context.Guild);
            if (emote != null)
            {
                cfg.EmoteMessageFormat = emote.MessageFormat;
                await ReplyAsync($"Set Hunger Game sign-up emote to {emote}!", Color.Green);
            }
            else
            {
                cfg.SignUpChannel = null;
                await ReplyAsync("Now using the default emote for sign-ups!", Color.Green);
            }

            await db.SaveChangesAsync();
        }

        [Name("Set sign-up channel")]
        [Command("hgschannel", "hgsc")]
        [Description("Sets the sign-up channel for hunger game")]
        [RequireMemberGuildPermissions(Permission.Administrator)]
        public async Task SetSignUpChannel(CachedTextChannel channel = null)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateHungerGameStatus(Context.Guild);
            if (channel != null)
            {
                cfg.SignUpChannel = channel.Id.RawValue;
                cfg.EventChannel ??= channel.Id.RawValue;
                await ReplyAsync($"Set Hunger Game sign-up channel to {channel.Mention}! This starts the game on schedule (every 6hrs)", Color.Green);
            }
            else
            {
                cfg.SignUpChannel = null;
                await ReplyAsync("Disabled sign-up channel, and also Hunger Game as a whole", Color.Green);
            }

            await db.SaveChangesAsync();
        }

        [Name("Set event channel")]
        [Command("hgechannel", "hgec")]
        [Description("Sets the event channel, where all the rounds will be outputted to")]
        [RequireMemberGuildPermissions(Permission.Administrator)]
        public async Task SetEventChannel(CachedTextChannel channel = null)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateHungerGameStatus(Context.Guild);
            if (channel != null)
            {
                cfg.EventChannel = channel.Id.RawValue;
                await ReplyAsync($"Set Hunger Game event channel to {channel.Mention}!", Color.Green);
            }
            else
            {
                cfg.EventChannel = null;
                if (cfg.SignUpChannel.HasValue) cfg.EventChannel = cfg.SignUpChannel.Value;
                await ReplyAsync("Disabled event channel, or set it to sign-up channel if there's one currently setup.", Color.Green);
            }

            await db.SaveChangesAsync();
        }

        [Name("Set sign-up message")]
        [Command("hgsm")]
        [Description("Sets message being sent during sign-up along with sign-up reaction")]
        [RequireMemberGuildPermissions(Permission.Administrator)]
        public async Task SetSignUpMessage([Remainder] string message = null)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateHungerGameStatus(Context.Guild);
            if (!message.IsNullOrWhiteSpace())
            {
                cfg.SignUpMessage = message;
                await ReplyAsync("Set Hunger Game sign-up message!", Color.Green);
            }
            else
            {
                cfg.SignUpMessage = null;
                await ReplyAsync("Now using the default message for sign-ups!", Color.Green);
            }

            await db.SaveChangesAsync();
        }

        [Name("Exp Reward")]
        [Command("hgexp")]
        [Description("Sets exp reward for winning the hunger games!")]
        [RequireMemberGuildPermissions(Permission.Administrator)]
        public async Task SetExpReward(int exp = 0)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateHungerGameStatus(Context.Guild);
            if (exp > 0)
            {
                cfg.ExpReward = exp;
                await ReplyAsync($"Set Hunger Game winning exp reward to {exp}!", Color.Green);
            }
            else
            {
                cfg.ExpReward = 0;
                await ReplyAsync("Disabled exp reward for winning Hunger Games!", Color.Green);
            }

            await db.SaveChangesAsync();
        }

        [Name("Credit Reward")]
        [Command("hgcredit")]
        [Description("Sets credit reward for winning the hunger games!")]
        [RequireMemberGuildPermissions(Permission.Administrator)]
        public async Task SetCreditReward(int credit = 0)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateHungerGameStatus(Context.Guild);
            if (credit > 0)
            {
                cfg.CreditReward = credit;
                await ReplyAsync($"Set Hunger Game winning credit reward to {credit}!", Color.Green);
            }
            else
            {
                cfg.CreditReward = 0;
                await ReplyAsync("Disabled credit reward for winning Hunger Games!", Color.Green);
            }

            await db.SaveChangesAsync();
        }

        [Name("Special Credit Reward")]
        [Command("hgscredit")]
        [Description("Sets special credit reward for winning the hunger games!")]
        [RequireMemberGuildPermissions(Permission.Administrator)]
        public async Task SetSpecialCreditAsync(int specialCredit = 0)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateHungerGameStatus(Context.Guild);
            if (specialCredit > 0)
            {
                cfg.SpecialCreditReward = specialCredit;
                await ReplyAsync($"Set Hunger Game winning special credit reward to {specialCredit}!", Color.Green);
            }
            else
            {
                cfg.SpecialCreditReward = 0;
                await ReplyAsync("Disabled special credit reward for winning Hunger Games!", Color.Green);
            }

            await db.SaveChangesAsync();
        }

        [Name("Role Reward")]
        [Command("hgrole")]
        [Description("Sets role reward for winning the hunger games!")]
        [RequireMemberGuildPermissions(Permission.Administrator)]
        public async Task SetRoleReward(CachedRole role = null)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateHungerGameStatus(Context.Guild);
            if (role != null)
            {
                cfg.RoleReward = role.Id.RawValue;
                await ReplyAsync($"Set Hunger Game winning role reward to {role.Mention}!", Color.Green);
            }
            else
            {
                cfg.RoleReward = null;
                await ReplyAsync("Disabled role reward for winning Hunger Games!", Color.Green);
            }

            await db.SaveChangesAsync();
        }
    }
}