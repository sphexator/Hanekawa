// using System.Threading.Tasks;
// using Disqord;
// using Disqord.Bot;
// using Disqord.Gateway;
// using Hanekawa.Bot.Commands.Preconditions;
// using Hanekawa.Database;
// using Hanekawa.Database.Extensions;
// using Hanekawa.Entities;
// using Hanekawa.Entities.Color;
// using Microsoft.Extensions.DependencyInjection;
// using Qmmands;
// using Quartz.Util;
// TODO: Hunger Game Commands
// namespace Hanekawa.Bot.Commands.Modules.Game
// {
//     [Name("Hunger Games")]
//     [Group("HungerGame", "hg")]
//     [Description("Commands to setup hunger games")]
//     [RequirePremium]
//     public class HungerGame : HanekawaCommandModule, IModuleSetting
//     {
//         [Name("Emote")]
//         [Command("emote")]
//         [Description("Sets the emote used to sign-up for hunger games")]
//         [RequireAuthorGuildPermissions(Permission.ManageGuild)]
//         public async Task SetSignUpEmoteAsync(IGuildEmoji emote)
//         {
//             await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
//             var cfg = await db.GetOrCreateHungerGameStatus(Context.Guild);
//             if (emote != null)
//             {
//                 cfg.EmoteMessageFormat = emote.GetMessageFormat();
//                 await Reply($"Set Hunger Game sign-up emote to {emote}!", HanaBaseColor.Ok());
//             }
//             else
//             {
//                 cfg.SignUpChannel = null;
//                 await Reply("Now using the default emote for sign-ups!", HanaBaseColor.Ok());
//             }
//
//             await db.SaveChangesAsync();
//         }
//
//         [Name("Sign-up Channel")]
//         [Command("schannel", "sc")]
//         [Description("Sets the sign-up channel for hunger games")]
//         [RequireAuthorGuildPermissions(Permission.ManageGuild)]
//         public async Task SetSignUpChannelAsync(ITextChannel channel)
//         {
//             await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
//             var cfg = await db.GetOrCreateHungerGameStatus(Context.Guild);
//             if (channel != null)
//             {
//                 cfg.SignUpChannel = channel.Id;
//                 cfg.EventChannel ??= channel.Id;
//                 await Reply($"Set Hunger Game sign-up channel to {channel.Mention}! This starts the game on schedule (every 6hrs)", HanaBaseColor.Ok());
//             }
//             else
//             {
//                 cfg.SignUpChannel = null;
//                 await Reply("Disabled sign-up channel, and also Hunger Game as a whole", HanaBaseColor.Ok());
//             }
//
//             await db.SaveChangesAsync();
//         }
//
//         [Name("Event Channel")]
//         [Command("echannel", "ec")]
//         [Description("Sets the event channel, where all the rounds will be outputted to")]
//         [RequireAuthorGuildPermissions(Permission.ManageGuild)]
//         public async Task SetEventChannelAsync(ITextChannel channel)
//         {
//             await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
//             var cfg = await db.GetOrCreateHungerGameStatus(Context.Guild);
//             if (channel != null)
//             {
//                 cfg.EventChannel = channel.Id;
//                 await Reply($"Set Hunger Game event channel to {channel.Mention}!", HanaBaseColor.Ok());
//             }
//             else
//             {
//                 cfg.EventChannel = null;
//                 if (cfg.SignUpChannel.HasValue) cfg.EventChannel = cfg.SignUpChannel.Value;
//                 await Reply("Disabled event channel, or set it to sign-up channel if there's one currently setup.", HanaBaseColor.Ok());
//             }
//
//             await db.SaveChangesAsync();
//         }
//
//         [Name("Message")]
//         [Command("message", "msg")]
//         [Description("Sets the sign-up message for hunger games")]
//         [RequireAuthorGuildPermissions(Permission.ManageGuild)]
//         public async Task SetSignupMessageAsync([Remainder] string message = null)
//         {
//             await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
//             var cfg = await db.GetOrCreateHungerGameStatus(Context.Guild);
//             if (!message.IsNullOrWhiteSpace())
//             {
//                 cfg.SignUpMessage = message;
//                 await Reply("Set Hunger Game sign-up message!", HanaBaseColor.Ok());
//             }
//             else
//             {
//                 cfg.SignUpMessage = null;
//                 await Reply("Now using the default message for sign-ups!", HanaBaseColor.Ok());
//             }
//
//             await db.SaveChangesAsync();
//         }
//         
//         [Name("Exp Reward")]
//         [Command("exp")]
//         [Description("Sets exp reward for winning the hunger games!")]
//         [RequireAuthorGuildPermissions(Permission.ManageGuild)]
//         public async Task SetExpRewardAsync(int exp = 0)
//         {
//             await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
//             var cfg = await db.GetOrCreateHungerGameStatus(Context.Guild);
//             if (exp > 0)
//             {
//                 cfg.ExpReward = exp;
//                 await Reply($"Set Hunger Game winning exp reward to {exp}!", HanaBaseColor.Ok());
//             }
//             else
//             {
//                 cfg.ExpReward = 0;
//                 await Reply("Disabled exp reward for winning Hunger Games!", HanaBaseColor.Ok());
//             }
//
//             await db.SaveChangesAsync();
//         }
//
//         [Name("Credit Reward")]
//         [Command("credit")]
//         [Description("Sets credit reward for winning the hunger games!")]
//         [RequireAuthorGuildPermissions(Permission.ManageGuild)]
//         public async Task SetCreditRewardAsync(int credit = 0)
//         {
//             await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
//             var cfg = await db.GetOrCreateHungerGameStatus(Context.Guild);
//             if (credit > 0)
//             {
//                 cfg.CreditReward = credit;
//                 await Reply($"Set Hunger Game winning credit reward to {credit}!", HanaBaseColor.Ok());
//             }
//             else
//             {
//                 cfg.CreditReward = 0;
//                 await Reply("Disabled credit reward for winning Hunger Games!", HanaBaseColor.Ok());
//             }
//
//             await db.SaveChangesAsync();
//         }
//
//         [Name("Special Credit Reward")]
//         [Command("specialCredit", "scredit")]
//         [Description("Sets special credit reward for winning the hunger games!")]
//         [RequireAuthorGuildPermissions(Permission.ManageGuild)]
//         public async Task SetSpecialCreditAsync(int specialCredit = 0)
//         {
//             await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
//             var cfg = await db.GetOrCreateHungerGameStatus(Context.Guild);
//             if (specialCredit > 0)
//             {
//                 cfg.SpecialCreditReward = specialCredit;
//                 await Reply($"Set Hunger Game winning special credit reward to {specialCredit}!", HanaBaseColor.Ok());
//             }
//             else
//             {
//                 cfg.SpecialCreditReward = 0;
//                 await Reply("Disabled special credit reward for winning Hunger Games!", HanaBaseColor.Ok());
//             }
//
//             await db.SaveChangesAsync();
//         }
//
//         [Name("Role Reward")]
//         [Command("role")]
//         [Description("Sets role reward for winning the hunger games!")]
//         [RequireAuthorGuildPermissions(Permission.ManageGuild)]
//         public async Task SetRoleRewardAsync(CachedRole role = null)
//         {
//             await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
//             var cfg = await db.GetOrCreateHungerGameStatus(Context.Guild);
//             if (role != null)
//             {
//                 cfg.RoleReward = role.Id;
//                 await Reply($"Set Hunger Game winning role reward to {role.Mention}!", HanaBaseColor.Ok());
//             }
//             else
//             {
//                 cfg.RoleReward = null;
//                 await Reply("Disabled role reward for winning Hunger Games!", HanaBaseColor.Ok());
//             }
//
//             await db.SaveChangesAsync();
//         }
//     }
// }