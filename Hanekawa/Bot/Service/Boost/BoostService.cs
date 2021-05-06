using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Config.Guild;
using Hanekawa.Entities;
using Hanekawa.Extensions;
using Hanekawa.Utility;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Hanekawa.Bot.Service.Boost
{
    public class BoostService : INService
    {
        private readonly Hanekawa _bot;
        private readonly Experience.ExpService _exp;
        private readonly IServiceProvider _provider;
        private readonly CacheService _cache;
        private readonly Logger _logger;

        public BoostService(Hanekawa bot, Experience.ExpService exp, IServiceProvider provider, CacheService cache)
        {
            _bot = bot;
            _exp = exp;
            _provider = provider;
            _cache = cache;
            _logger = LogManager.GetCurrentClassLogger();
        }

        public async Task BoostCheckAsync(MemberUpdatedEventArgs e)
        {
            if (!e.OldMember.BoostedAt.HasValue && e.NewMember.BoostedAt.HasValue) await StartedBoostingAsync(e.NewMember);
            if (e.OldMember.BoostedAt.HasValue && !e.NewMember.BoostedAt.HasValue) await EndedBoostingAsync(e.NewMember);
        }

        private async Task Reward()
        {
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var configs = await db.BoostConfigs.ToListAsync();
            foreach (var x in configs.Where(x => x.ExpGain != 0 || x.CreditGain != 0 || x.SpecialCreditGain != 0))
            {
                try
                {
                    var guild = _bot.GetGuild(x.GuildId);
                    if (guild == null) continue;
                    await RewardGuildAsync(guild, db, x);
                }
                catch (Exception e)
                { 
                    _logger.Log(LogLevel.Error, e,
                        $"(Boost Service) Error in {x.GuildId} when rewarding users - {e.Message}");
                }
            }

            _logger.Log(LogLevel.Info,
                "(Boost Service) Finished rewarding users in all guilds configured for it");
        }

        private async Task RewardGuildAsync(IGatewayGuild guild, DbService db, BoostConfig x)
        {
            var users = guild.Members.Where(u => u.Value.BoostedAt.HasValue).ToList();
            var currencyCfg = await db.GetOrCreateCurrencyConfigAsync(guild);
            foreach (var (_, member) in users)
            {
                try
                {
                    await RewardUserAsync(db, member, x, guild, currencyCfg);
                }
                catch (Exception e)
                {
                    _logger.Log(LogLevel.Error, e,
                        $"(Boost Service) Error in {x.GuildId} when rewarding {member.Id.RawValue} - {e.Message}");
                }
            }

            await db.SaveChangesAsync();
            _logger.Log(LogLevel.Info, $"Rewarded {users.Count} boosters in {guild.Id.RawValue}");
        }

        private async Task RewardUserAsync(DbService db, IMember member, BoostConfig x, IGuild guild,
            CurrencyConfig currencyCfg)
        {
            var userData = await db.GetOrCreateUserData(member);
            var exp = await _exp.AddExpAsync(member, userData, x.ExpGain, x.CreditGain, db, ExpSource.Other);
            userData.CreditSpecial += x.SpecialCreditGain;
            var sb = new StringBuilder();

            sb.AppendLine(
                member.BoostedAt != null
                    ? $"Thank you for boosting {guild.Name} for {(DateTimeOffset.UtcNow - member.BoostedAt.Value).Humanize(2)}!"
                    : $"Thank you for boosting {guild.Name}!");

            sb.AppendLine("You've been rewarded:");
            if (x.ExpGain != 0 && exp != 0) sb.AppendLine($"Experience: {exp}");
            if (x.CreditGain != 0)
                sb.AppendLine(
                    $"{currencyCfg.CurrencyName}: {currencyCfg.ToCurrencyFormat(x.CreditGain)}");
            if (x.SpecialCreditGain != 0)
                sb.AppendLine(
                    $"{currencyCfg.SpecialCurrencyName}: {currencyCfg.ToCurrencyFormat(x.SpecialCreditGain, true)}");
            try
            {
                await member.SendMessageAsync(new LocalMessageBuilder
                {
                    Embed = new LocalEmbedBuilder
                    {
                        Author = new LocalEmbedAuthorBuilder
                        {
                            Name = $"{guild.Name} Boost Rewards",
                            IconUrl = guild.GetIconUrl()

                        },
                        Color = _cache.GetColor(guild.Id.RawValue),
                        Description = sb.ToString()
                    },
                    Attachments = null,
                    Content = null,
                    Mentions = LocalMentionsBuilder.None,
                    Reference = null,
                    IsTextToSpeech = false
                }.Build());
            }
            catch { /* User likely has DMs blocked */ }
        }

        private async Task StartedBoostingAsync(IMember user)
        {
            try
            {
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var userData = await db.GetOrCreateUserData(user);
                var config = await db.GetOrCreateBoostConfigAsync(user.GuildId);
                await _exp.AddExpAsync(user, userData, config.ExpGain, config.CreditGain, db, ExpSource.Other);
                if (config.SpecialCreditGain > 0) userData.CreditSpecial += config.SpecialCreditGain;
                await db.SaveChangesAsync();
                var guild = _bot.GetGuild(user.GuildId);
                if (config.ChannelId.HasValue)
                {
                    var channel = guild.GetChannel(config.ChannelId.Value);
                    if (channel != null)
                    {
                        var embed = new LocalEmbedBuilder
                        {
                            Author = new LocalEmbedAuthorBuilder
                            {
                                Name = $"{user} Boosted the server!",
                                IconUrl = user.GetAvatarUrl()
                            },
                            Description = MessageUtil.FormatMessage(config.Message, user, guild),
                            ThumbnailUrl = user.GetAvatarUrl()
                        };
                        await _bot.SendMessageAsync(channel.Id, new LocalMessageBuilder
                        {
                            Embed = embed,
                            Attachments = null,
                            Content = null,
                            Mentions = LocalMentionsBuilder.None,
                            Reference = null,
                            IsTextToSpeech = false
                        }.Build());
                    }
                }

                var logCfg = await db.GetOrCreateLoggingConfigAsync(user.GuildId);
                if (!logCfg.LogAvi.HasValue) return;
                var logChannel = guild.GetChannel(logCfg.LogAvi.Value);
                if (logChannel == null) return;
                var logEmbed = new LocalEmbedBuilder
                {
                    Title = "Started Boosting",
                    Description = $"{user.Mention} has started boosting the server!",
                    Color = _cache.GetColor(user.GuildId),
                    Timestamp = DateTimeOffset.UtcNow,
                    Footer = new LocalEmbedFooterBuilder
                    {
                        Text = $"Username: {user} ({user.Id})",
                        IconUrl = user.GetAvatarUrl()
                    }
                };
                await _bot.SendMessageAsync(logChannel.Id, new LocalMessageBuilder
                {
                    Embed = logEmbed,
                    Attachments = null,
                    Content = null,
                    Mentions = LocalMentionsBuilder.None,
                    Reference = null,
                    IsTextToSpeech = false
                }.Build());
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, e, $"(Boost Service) Error for start boosting in {user.GuildId.RawValue} for {user.Id.RawValue} - {e.Message}");
            }
        }
        
        private async Task EndedBoostingAsync(IMember user)
        {
            try
            {
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var logCfg = await db.GetOrCreateLoggingConfigAsync(user.GuildId);
                if (!logCfg.LogAvi.HasValue) return;
                var guild = _bot.GetGuild(user.GuildId);
                var channel = guild.GetChannel(logCfg.LogAvi.Value);
                if (channel == null) return;
                var embed = new LocalEmbedBuilder
                {
                    Title = "Stopped Boosting",
                    Description = $"{user.Mention} has stopped boosting the server!",
                    Color = _cache.GetColor(user.GuildId),
                    Timestamp = DateTimeOffset.UtcNow,
                    Footer = new LocalEmbedFooterBuilder
                    {
                        Text = $"User: {user} ({user.Id})",
                        IconUrl = user.GetAvatarUrl()
                    }
                };
                await _bot.SendMessageAsync(channel.Id, new LocalMessageBuilder
                {
                    Embed = embed,
                    Attachments = null,
                    Content = null,
                    Mentions = LocalMentionsBuilder.None,
                    Reference = null,
                    IsTextToSpeech = false
                }.Build());
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, e, $"(Boost Service) Error for end boosting in {user.GuildId.RawValue} for {user.Id.RawValue} - {e.Message}");
            }
        }
    }
}