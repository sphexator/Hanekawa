using System;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Extensions;
using Hanekawa.Preconditions;
using Hanekawa.Services.Entities;
using Hanekawa.Services.Entities.Tables;
using Humanizer;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Modules.Account
{
    public class Economy : InteractiveBase
    {
        [Command("wallet")]
        [Alias("balance", "money")]
        [Summary("Display how much credit you got")]
        [Ratelimit(1, 2, Measure.Seconds)]
        [RequiredChannel]
        public async Task WalletAsync(SocketGuildUser user = null)
        {
            if(user == null) user = Context.User as SocketGuildUser;
            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(user);
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                var author = new EmbedAuthorBuilder
                {
                    IconUrl = user.GetAvatar(),
                    Name = user.GetName()
                };
                var embed = new EmbedBuilder
                {
                    Author = author,
                    Color = Color.Purple,
                    Description = $"{GetRegularCurrency(userdata, cfg)}\n" +
                                  $"{GetSpecialCurrency(userdata, cfg)}"
                };
                await ReplyAsync(null, false, embed.Build());
            }
        }

        [Command("give", RunMode = RunMode.Async)]
        [Alias("transfer")]
        [Summary("Transfer credit between users")]
        [Ratelimit(1, 2, Measure.Seconds)]
        [RequiredChannel]
        public async Task GiveCreditAsync(uint amount, SocketGuildUser user)
        {
            if (user == Context.User) return;
            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(Context.User as SocketGuildUser);
                var recieverData = await db.GetOrCreateUserData(user);
                if (userdata.Credit < amount)
                {
                    await ReplyAndDeleteAsync(null, false, new EmbedBuilder().Reply($"{Context.User.Mention} doesn't have enough credit", Color.Red.RawValue).Build());
                    return;
                }

                userdata.Credit = userdata.Credit - amount;
                recieverData.Credit = recieverData.Credit + amount;
                await db.SaveChangesAsync();
                await ReplyAndDeleteAsync(null, false, new EmbedBuilder().Reply($"{Context.User.Mention} transferred ${amount} to {user.Mention}", Color.Green.RawValue).Build());
            }
        }

        [Command("daily", RunMode = RunMode.Async)]
        [Summary("Daily credit command, usable once every 18hrs")]
        [Ratelimit(1, 2, Measure.Seconds)]
        [RequiredChannel]
        public async Task DailyAsync(SocketGuildUser user = null)
        {
            using (var db = new DbService())
            {
                var cooldownCheckAccount = await db.GetOrCreateUserData(Context.User as SocketGuildUser);
                if (cooldownCheckAccount.DailyCredit.AddHours(18) >= DateTime.UtcNow)
                {
                    var timer = cooldownCheckAccount.DailyCredit.AddHours(18) - DateTime.UtcNow;
                    await ReplyAndDeleteAsync(null, false, new EmbedBuilder().Reply($"{Context.User.Mention} daily credit refresh in {timer.Humanize()}", Color.Red.RawValue).Build());
                    return;
                }

                uint reward;
                if (user == null || user == Context.User)
                {
                    user = Context.User as SocketGuildUser;
                    reward = 200;
                    var userdata = await db.GetOrCreateUserData(user);
                    userdata.DailyCredit = DateTime.UtcNow;
                    userdata.Credit = userdata.Credit + reward;
                    await db.SaveChangesAsync();
                    await ReplyAndDeleteAsync(null, false, new EmbedBuilder().Reply($"rewarded {user?.Mention} with ${reward} credit", Color.Green.RawValue).Build());
                }
                else
                {
                    reward = Convert.ToUInt32(new Random().Next(200, 300));
                    var userData = await db.GetOrCreateUserData(Context.User as SocketGuildUser);
                    var recieverData = await db.GetOrCreateUserData(user);
                    userData.DailyCredit = DateTime.UtcNow;
                    recieverData.Credit = recieverData.Credit + reward;
                    await db.SaveChangesAsync();
                    await ReplyAndDeleteAsync(null, false, new EmbedBuilder().Reply($"{Context.User.Mention} rewarded {user.Mention} with ${reward} credit", Color.Green.RawValue).Build());
                }
            }
        }

        [Command("richest", RunMode = RunMode.Async)]
        [Ratelimit(1, 5, Measure.Seconds)]
        [Summary("Displays top 10 users on the money leaderboard")]
        [RequiredChannel]
        public async Task LeaderboardAsync()
        {
            using (var db = new DbService())
            {
                var author = new EmbedAuthorBuilder
                {
                    Name = $"Money leaderboard for {Context.Guild.Name}",
                    IconUrl = Context.Guild.IconUrl
                };
                var embed = new EmbedBuilder
                {
                    Color = Color.Purple,
                    Author = author
                };
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                var users = await db.Accounts.Where(x => x.Active && x.GuildId == Context.Guild.Id).OrderByDescending(account => account.Credit).Take(10).ToListAsync();
                var rank = 1;
                foreach (var x in users)
                {
                    var field = new EmbedFieldBuilder
                    {
                        IsInline = false,
                        Name = $"Rank: {rank}",
                        Value = $"<@{x.UserId}> - {cfg.CurrencyName}:{x.Credit}"
                    };
                    embed.AddField(field);
                    rank++;
                }
                await ReplyAsync(null, false, embed.Build());
            }
        }

        [Command("reward", RunMode = RunMode.Async)]
        [Alias("award")]
        [Summary("Rewards special credit to users (does not remove from yourself)")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task RewardCreditAsync(uint amount, IGuildUser user)
        {
            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateUserData(user as SocketGuildUser);
                var cfg = await db.GetOrCreateGuildConfig(Context.Guild);
                userdata.CreditSpecial = userdata.CreditSpecial + amount;
                await db.SaveChangesAsync();
                await ReplyAsync(null, false, new EmbedBuilder().Reply($"Rewarded {SpecialCurrencyResponse(cfg)}{amount} {cfg.SpecialCurrencyName} to {user.Mention}", Color.Green.RawValue).Build());
            }
        }

        private static string SpecialCurrencyResponse(GuildConfig cfg)
        {
            return cfg.SpecialEmoteCurrency ? $"{CurrencySignEmote(cfg.SpecialCurrencySign)}" : cfg.SpecialCurrencySign;
        }

        private static string GetRegularCurrency(Services.Entities.Tables.Account userdata, GuildConfig cfg)
        {
            return cfg.EmoteCurrency ? RegularEmoteCurrency(userdata, cfg) : RegularTextCurrency(userdata, cfg);
        }

        private static string RegularEmoteCurrency(Services.Entities.Tables.Account userdata, GuildConfig cfg)
        {
            return $"{cfg.CurrencyName}: {CurrencySignEmote(cfg.CurrencySign)}{userdata.Credit}";
        }

        private static string RegularTextCurrency(Services.Entities.Tables.Account userdata, GuildConfig cfg)
        {
            return $"{cfg.CurrencyName}: {cfg.CurrencySign}{userdata.Credit}";
        }

        private static string GetSpecialCurrency(Services.Entities.Tables.Account userdata, GuildConfig cfg)
        {
            return cfg.SpecialEmoteCurrency ? SpecialEmoteCurrency(userdata, cfg) : SpecialTextCurrency(userdata, cfg);
        }

        private static string SpecialEmoteCurrency(Services.Entities.Tables.Account userdata, GuildConfig cfg)
        {
            return $"{cfg.CurrencyName}: {CurrencySignEmote(cfg.CurrencySign)}{userdata.CreditSpecial}";
        }

        private static string SpecialTextCurrency(Services.Entities.Tables.Account userdata, GuildConfig cfg)
        {
            return $"{cfg.CurrencyName}: {cfg.CurrencySign}{userdata.CreditSpecial}";
        }

        private static IEmote CurrencySignEmote(string emoteString)
        {
            if (Emote.TryParse(emoteString, out var emote)) return emote;
            else
            {
                Emote.TryParse("<a:wawa:475462796214009856>", out var defaultEmote);
                return defaultEmote;
            }
        }
    }
}