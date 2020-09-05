using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Events;
using Hanekawa.Bot.Services.Experience;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Shared.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Bot.Services.Boost
{
    public class BoostService : IRequired, INService
    {
        private readonly Hanekawa _client;
        private readonly ExpService _exp;
        private readonly IServiceProvider _provider;
        public BoostService(Hanekawa client, IServiceProvider provider, ExpService exp)
        {
            _client = client;
            _provider = provider;
            _exp = exp;

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
                            Name = user.DisplayName,
                            IconUrl = user.GetAvatarUrl()
                        },
                        Description = config.Message,
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
                    Text = $"User: {user} ({user.Id})",
                    IconUrl = user.GetAvatarUrl()
                }
            };
            await logChannel.SendMessageAsync(null, false, logEmbed.Build(), LocalMentions.NoEveryone);
        }

        private async Task EndedBoostingAsync(CachedMember user)
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
    }
}