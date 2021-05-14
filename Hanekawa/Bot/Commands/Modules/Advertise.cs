using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Rest;
using Hanekawa.Database;
using Hanekawa.Database.Tables.Advertise;
using Hanekawa.Entities;
using Hanekawa.Entities.Color;
using Hanekawa.Extensions;
using Hanekawa.Utility;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Commands.Modules
{
    [Name("Advertisement")]
    [Description("Commands for advertisement")]
    [Group("Advertise", "ad")]
    public class Advertise : HanekawaCommandModule, IModuleSetting
    {
        [Name("Create")]
        [Description("Creates a config and key in-order to take in Top.gg requests to reward users for voting!")]
        [Command("create")]
        [RequireAuthorGuildPermissions(Permission.Administrator)]
        public async Task CreateTopWebhookAsync()
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var check = await db.DblAuths.FindAsync(Context.Guild.Id);
            if (check != null)
            {
                await Reply("You've already made a config!, sending new key in dms");
                try
                {
                    await Context.Author.SendMessageAsync(new LocalMessageBuilder().Create(
                        $"Your top.gg Webhook URL is: https://hanekawa.bot/api/advert/dbl \nAuth Key: {check.AuthKey}",
                        HanaBaseColor.Ok()));
                }
                catch
                {
                    await Reply(
                        $"Could not DM, sending here. Please delete this message afterwards.\nWebhook URL: https://hanekawa.bot/api/advert/dbl \nAuth Key: {check.AuthKey}",
                        HanaBaseColor.Ok());
                }
                return;
            }

            await db.DblAuths.AddAsync(new DblAuth
            {
                GuildId = Context.Guild.Id,
                ExpGain = 0,
                CreditGain = 0,
                SpecialCredit = 0,
                RoleIdReward = null,
                Message = null,
                AuthKey = Guid.NewGuid()
            });
            await db.SaveChangesAsync();
            var cfg = await db.DblAuths.FindAsync(Context.Guild.Id);
            await Reply("Authentication Created! DMing the credentials.");
            try
            {
                await Context.Author.SendMessageAsync(new LocalMessageBuilder().Create("Your top.gg Webhook URL is: https://hanekawa.bot/api/advert/dbl\n" +
                                               $"Auth Key: {cfg.AuthKey}", HanaBaseColor.Ok()));
            }
            catch
            {
                await Reply("Could not DM, sending here. Please delete this message afterwards.\n" +
                                 "Webhook URL: https://hanekawa.bot/api/advert/dbl \n" +
                                 $"Auth Key: {cfg.AuthKey}", HanaBaseColor.Ok());
            }
        }
        
        [Name("Set Exp Reward")]
        [Description("Sets exp reward for voting on the server on top.gg")]
        [Command("exp")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public async Task AddExpReward(int exp = 0)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.DblAuths.FindAsync(Context.Guild.Id.RawValue);
            if (cfg == null)
            {
                await Reply("Please create a webhook & config first before using these commands!", Color.Red);
                return;
            }
            cfg.ExpGain = exp;
            await db.SaveChangesAsync();
            if (exp == 0)
            {
                await Reply("Disabled exp reward for Top.gg votes");
            }
            else
            {
                await Reply($"Set Top.gg exp vote rewards to {exp}");
            }
        }

        [Name("Set Credit Reward")]
        [Description("Sets credit reward for voting on the server on top.gg")]
        [Command("credit")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public async Task AddCreditReward(int credit = 0)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.DblAuths.FindAsync(Context.Guild.Id.RawValue);
            if (cfg == null)
            {
                await Reply("Please create a webhook & config first before using these commands!", Color.Red);
                return;
            }
            cfg.CreditGain = credit;
            await db.SaveChangesAsync();
            if (credit == 0)
            {
                await Reply("Disabled credit reward for Top.gg votes");
            }
            else
            {
                await Reply($"Set Top.gg credit vote rewards to {credit}");
            }
        }

        [Name("Set Special Credit Reward")]
        [Description("Sets special credit reward for voting on the server on top.gg")]
        [Command("scredit")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public async Task AddSpecialReward(int specialCredit = 0)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.DblAuths.FindAsync(Context.Guild.Id.RawValue);
            if (cfg == null)
            {
                await Reply("Please create a webhook & config first before using these commands!", Color.Red);
                return;
            }
            cfg.SpecialCredit = specialCredit;
            await db.SaveChangesAsync();
            if (specialCredit == 0)
            {
                await Reply("Disabled special credit reward for Top.gg votes");
            }
            else
            {
                await Reply($"Set Top.gg special credit vote rewards to {specialCredit}");
            }
        }

        [Name("Set Role Reward")]
        [Description("Sets role reward for voting on the server on top.gg")]
        [Command("role")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public async Task AddRoleReward(IRole role = null)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.DblAuths.FindAsync(Context.Guild.Id.RawValue);
            if (cfg == null)
            {
                await Reply("Please create a webhook & config first before using these commands!", Color.Red);
                return;
            }
            cfg.RoleIdReward = role?.Id.RawValue;
            await db.SaveChangesAsync();
            if (role == null)
            {
                await Reply("Disabled role reward for Top.gg vote");
            }
            else
            {
                await Reply($"Set Top.gg role rewards to {role.Name}");
            }
        }

        [Name("Set Notification")]
        [Description("Sets message to be sent in dms for voting on the server on top.gg")]
        [Command("message", "msg")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild)]
        public async Task AddMessage(string message = null)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.DblAuths.FindAsync(Context.Guild.Id.RawValue);
            if (cfg == null)
            {
                await Reply("Please create a webhook & config first before using these commands!", Color.Red);
                return;
            }
            cfg.Message = message;
            await db.SaveChangesAsync();
            if (message == null)
            {
                await Reply("Disabled dm notification when user has voted");
            }
            else
            {
                await Reply("Set DM notification message to:\n" +
                            $"{MessageUtil.FormatMessage(message, Context.Author, Context.Guild)}");
            }
        }
    }
}