using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Events;
using Hanekawa.Bot.Services.Experience;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Shared.Interfaces;
using Hanekawa.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Hanekawa.Bot.Services.Boost
{
    public class BoostService : IRequired, INService, IJob
    {
        private readonly Hanekawa _client;
        private readonly ExpService _exp;
        private readonly IServiceProvider _provider;
        private readonly InternalLogService _log;

        public BoostService(Hanekawa client, IServiceProvider provider, ExpService exp, InternalLogService log)
        {
            _client = client;
            _provider = provider;
            _exp = exp;
            _log = log;

            _client.MemberUpdated += BoostCheck;
        }

        private Task BoostCheck(MemberUpdatedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if (!e.OldMember.IsBoosting && e.NewMember.IsBoosting) await StartedBoostingAsync(e.NewMember);
                if (e.OldMember.IsBoosting && !e.NewMember.IsBoosting) await EndedBoostingAsync(e.NewMember);
            });
            return Task.CompletedTask;
        }

        private async Task StartedBoostingAsync(CachedMember user)
        {
            try
            {
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var userData = await db.GetOrCreateUserData(user);
                var config = await db.GetOrCreateBoostConfigAsync(user.Guild);
                await _exp.AddExpAsync(user, userData, config.ExpGain, config.CreditGain, db);
                if (config.SpecialCreditGain > 0) userData.CreditSpecial += config.SpecialCreditGain;
                await db.SaveChangesAsync();
                if (config.ChannelId.HasValue)
                {
                    var channel = user.Guild.GetTextChannel(config.ChannelId.Value);
                    if (channel != null)
                    {
                        var embed = new LocalEmbedBuilder
                        {
                            Author = new LocalEmbedAuthorBuilder
                            {
                                Name = $"{user.DisplayName} Boosted the server!",
                                IconUrl = user.GetAvatarUrl()
                            },
                            Description = MessageUtil.FormatMessage(config.Message, user, user.Guild),
                            ThumbnailUrl = user.GetAvatarUrl()
                        };
                        await channel.SendMessageAsync(null, false, embed.Build(), LocalMentions.NoEveryone);
                    }
                }

                var logCfg = await db.GetOrCreateLoggingConfigAsync(user.Guild);
                if (!logCfg.LogAvi.HasValue) return;
                var logChannel = user.Guild.GetTextChannel(logCfg.LogAvi.Value);
                if (logChannel == null) return;
                var logEmbed = new LocalEmbedBuilder
                {
                    Author = new LocalEmbedAuthorBuilder
                    {
                        Name = "Stopped boosting"
                    },
                    Description = $"{user.Mention} has stopped boosting the server!",
                    Color = Color.Purple,
                    Timestamp = DateTimeOffset.UtcNow,
                    Footer = new LocalEmbedFooterBuilder
                    {
                        Text = $"Username: {user} ({user.Id})",
                        IconUrl = user.GetAvatarUrl()
                    }
                };
                await logChannel.SendMessageAsync(null, false, logEmbed.Build(), LocalMentions.NoEveryone);
            }
            catch (Exception e)
            {
                _log.LogAction(LogLevel.Error, e, $"(Boost Service) Error for start boosting in {user.Guild.Id.RawValue} for {user.Id.RawValue} - {e.Message}");
            }
        }

        private async Task EndedBoostingAsync(CachedMember user)
        {
            try
            {
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var logCfg = await db.GetOrCreateLoggingConfigAsync(user.Guild);
                if (!logCfg.LogAvi.HasValue) return;
                var channel = user.Guild.GetTextChannel(logCfg.LogAvi.Value);
                if (channel == null) return;
                var embed = new LocalEmbedBuilder
                {
                    Author = new LocalEmbedAuthorBuilder
                    {
                        Name = "Stopped boosting"
                    },
                    Description = $"{user.Mention} has stopped boosting the server!",
                    Color = Color.Purple,
                    Timestamp = DateTimeOffset.UtcNow,
                    Footer = new LocalEmbedFooterBuilder
                    {
                        Text = $"User: {user} ({user.Id})",
                        IconUrl = user.GetAvatarUrl()
                    }
                };
                await channel.SendMessageAsync(null, false, embed.Build(), LocalMentions.NoEveryone);
            }
            catch (Exception e)
            {
                _log.LogAction(LogLevel.Error, e, $"(Boost Service) Error for end boosting in {user.Guild.Id.RawValue} for {user.Id.RawValue} - {e.Message}");
            }
        }

        public Task Execute(IJobExecutionContext context)
        {
            _ = Reward();
            return Task.CompletedTask;
        }

        private async Task Reward()
        {
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var configs = await db.BoostConfigs.ToListAsync();
            for (var i = 0; i < configs.Count; i++)
            {
                var x = configs[i];
                try
                {
                    var guild = _client.GetGuild(x.GuildId);
                    if (guild == null) continue;
                    var users = guild.Members.Where(x => x.Value.IsBoosting).ToList();
                    foreach (var (_, member) in users)
                    {
                        try
                        {
                            var userData = await db.GetOrCreateUserData(member);
                            await _exp.AddExpAsync(member, userData, x.ExpGain, x.CreditGain, db);
                            userData.CreditSpecial += x.SpecialCreditGain;
                        }
                        catch (Exception e)
                        {
                            _log.LogAction(LogLevel.Error, e,
                                $"(Boost Service) Error in {x.GuildId} when rewarding {member.Id.RawValue} - {e.Message}");
                        }
                    }

                    await db.SaveChangesAsync();
                    _log.LogAction(LogLevel.Information, $"Rewarded {users.Count} boosters in {guild.Id.RawValue}");
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e,
                        $"(Boost Service) Error in {x.GuildId} when rewarding users - {e.Message}");
                }
            }

            _log.LogAction(LogLevel.Information,
                $"(Boost Service) Finished rewarding users in all guilds configured for it");
        }
    }
}