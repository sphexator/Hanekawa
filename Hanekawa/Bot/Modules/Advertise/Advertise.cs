using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Database;
using Hanekawa.Database.Tables.Advertise;
using Hanekawa.Shared.Command;
using Hanekawa.Utility;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Modules.Advertise
{
    [Name("Advertise")]
    [Description("Commands to setup rewards for voting on the server on sites such as top.gg")]
    [RequireUser(111123736660324352)]
    public class Advertise : HanekawaCommandModule
    {
        [Name("Create TopGG Webhook")]
        [Description("Creates a config and key in-order to take in topgg requests to reward users for voting")]
        [Command("ctopwebhook")]
        [RequireMemberGuildPermissions(Permission.Administrator)]
        public async Task CreateTopGGWebhook()
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var check = await db.DblAuths.FindAsync(Context.Guild.Id.RawValue);
            var channel = Context.Member.DmChannel ?? (IDmChannel)await Context.Member.CreateDmChannelAsync();
            if (check != null)
            {
                await ReplyAsync("You've already made a config!, sending new key in dms");
                try
                {
                    await channel.SendMessageAsync("Your top.gg Webhook URL is: https://hanekawa.bot/api/advert/dbl \n" +
                                                   $"Auth Key: {check.AuthKey}");
                }
                catch
                {
                    await ReplyAsync("Could not DM, sending here. Please delete this message afterwards.\n" +
                                     "Webhook URL: https://hanekawa.bot/api/advert/dbl \n" +
                                     $"Auth Key: {check.AuthKey}", Color.Green);
                }
                return;
            }

            await db.DblAuths.AddAsync(new DblAuth
            {
                GuildId = Context.Guild.Id.RawValue,
                ExpGain = 0,
                CreditGain = 0,
                SpecialCredit = 0,
                RoleIdReward = null,
                Message = null,
                AuthKey = Guid.NewGuid()
            });
            await db.SaveChangesAsync();
            var cfg = await db.DblAuths.FindAsync(Context.Guild.Id.RawValue);
            await ReplyAsync("Authentication Created! DMing the credentials.");
            try
            {
                await channel.SendMessageAsync("Your top.gg Webhook URL is: https://hanekawa.bot/api/advert/dbl\n" +
                                               $"Auth Key: {cfg.AuthKey}");
            }
            catch
            {
                await ReplyAsync("Could not DM, sending here. Please delete this message afterwards.\n" +
                                 "Webhook URL: https://hanekawa.bot/api/advert/dbl \n" +
                                 $"Auth Key: {cfg.AuthKey}", Color.Green);
            }
        }

        [Name("Set Vote Exp Reward")]
        [Description("Sets exp reward for voting on the server on top.gg")]
        [Command("advertexp")]
        [RequireMemberGuildPermissions(Permission.Administrator)]
        public async Task AddExpReward(int exp = 0)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.DblAuths.FindAsync(Context.Guild.Id.RawValue);
            if (cfg == null)
            {
                await ReplyAsync("Please create a webhook & config first before using these commands!", Color.Red);
                return;
            }
            cfg.ExpGain = exp;
            await db.SaveChangesAsync();
            if (exp == 0)
            {
                await ReplyAsync("Disabled exp reward for Top.gg votes");
            }
            else
            {
                await ReplyAsync($"Set Top.gg exp vote rewards to {exp}");
            }
        }

        [Name("Set Vote Credit Reward")]
        [Description("Sets credit reward for voting on the server on top.gg")]
        [Command("advertcredit")]
        [RequireMemberGuildPermissions(Permission.Administrator)]
        public async Task AddCreditReward(int credit = 0)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.DblAuths.FindAsync(Context.Guild.Id.RawValue);
            if (cfg == null)
            {
                await ReplyAsync("Please create a webhook & config first before using these commands!", Color.Red);
                return;
            }
            cfg.CreditGain = credit;
            await db.SaveChangesAsync();
            if (credit == 0)
            {
                await ReplyAsync("Disabled credit reward for Top.gg votes");
            }
            else
            {
                await ReplyAsync($"Set Top.gg credit vote rewards to {credit}");
            }
        }

        [Name("Set Vote Special Credit Reward")]
        [Description("Sets special credit reward for voting on the server on top.gg")]
        [Command("advertscredit")]
        [RequireMemberGuildPermissions(Permission.Administrator)]
        public async Task AddSpecialReward(int specialCredit = 0)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.DblAuths.FindAsync(Context.Guild.Id.RawValue);
            if (cfg == null)
            {
                await ReplyAsync("Please create a webhook & config first before using these commands!", Color.Red);
                return;
            }
            cfg.SpecialCredit = specialCredit;
            await db.SaveChangesAsync();
            if (specialCredit == 0)
            {
                await ReplyAsync("Disabled special credit reward for Top.gg votes");
            }
            else
            {
                await ReplyAsync($"Set Top.gg special credit vote rewards to {specialCredit}");
            }
        }

        [Name("Set Vote Role Reward")]
        [Description("Sets role reward for voting on the server on top.gg")]
        [Command("advertrole")]
        [RequireMemberGuildPermissions(Permission.Administrator)]
        public async Task AddRoleReward(CachedRole role = null)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.DblAuths.FindAsync(Context.Guild.Id.RawValue);
            if (cfg == null)
            {
                await ReplyAsync("Please create a webhook & config first before using these commands!", Color.Red);
                return;
            }
            cfg.RoleIdReward = role?.Id.RawValue;
            await db.SaveChangesAsync();
            if (role == null)
            {
                await ReplyAsync("Disabled role reward for Top.gg vote");
            }
            else
            {
                await ReplyAsync($"Set Top.gg role rewards to {role.Name}");
            }
        }

        [Name("Set Vote Notification")]
        [Description("Sets message to be sent in dms for voting on the server on top.gg")]
        [Command("advertmessage", "advertmsg")]
        [RequireMemberGuildPermissions(Permission.Administrator)]
        public async Task AddMessage(string message = null)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.DblAuths.FindAsync(Context.Guild.Id.RawValue);
            if (cfg == null)
            {
                await ReplyAsync("Please create a webhook & config first before using these commands!", Color.Red);
                return;
            }
            cfg.Message = message;
            await db.SaveChangesAsync();
            if (message == null)
            {
                await ReplyAsync("Disabled dm notification when user has voted");
            }
            else
            {
                await ReplyAsync("Set DM notification message to:\n" +
                                 $"{MessageUtil.FormatMessage(message, Context.Member, Context.Guild)}");
            }
        }
    }
}
