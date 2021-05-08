using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Hanekawa.Bot.Service.Logs
{
    public partial class LogService
    {
        public async Task MemberUpdatedAsync(MemberUpdatedEventArgs e)
        {
            var guild = _bot.GetGuild(e.NewMember.GuildId);
            try
            {
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var cfg = await db.GetOrCreateLoggingConfigAsync(guild.Id.RawValue);
                if (!cfg.LogAvi.HasValue) return;
                var channel = guild.GetChannel(cfg.LogAvi.Value);
                if (channel == null) return;

                var embed = new LocalEmbedBuilder{
                    Footer = new LocalEmbedFooterBuilder { Text = $"Username: {e.NewMember} ({e.NewMember.Id.RawValue})", IconUrl = guild.GetIconUrl() }};
                if (e.OldMember.Nick != e.NewMember.Nick)
                {
                    embed.Author = new LocalEmbedAuthorBuilder
                        { Name = "Nickname Change", IconUrl = e.NewMember.GetAvatarUrl() };
                    embed.AddField("Old Nick", e.OldMember.Nick ?? e.NewMember.Name);
                    embed.AddField("New Nick", e.NewMember.Nick ?? e.NewMember.Name);
                }

                if (e.OldMember.Name != e.NewMember.Name)
                {
                    embed.Author = new LocalEmbedAuthorBuilder
                        { Name = "Name Change", IconUrl = e.NewMember.GetAvatarUrl() };
                    embed.AddField("Old Name", e.OldMember.Nick ?? e.NewMember.Name);
                    embed.AddField("New Name", e.NewMember.Nick ?? e.NewMember.Name);
                }

                if (e.OldMember.AvatarHash != e.NewMember.AvatarHash)
                {
                    embed.Author = new LocalEmbedAuthorBuilder
                        { Name = "Avatar Change", IconUrl = e.NewMember.GetAvatarUrl() };
                    embed.ThumbnailUrl = e.OldMember.GetAvatarUrl();
                    embed.ImageUrl = e.NewMember.GetAvatarUrl();
                }

                if(embed.Author == null) return;
                await _bot.SendMessageAsync(channel.Id, new LocalMessageBuilder
                {
                    Embed = embed,
                    IsTextToSpeech = false,
                    Mentions = LocalMentionsBuilder.None,
                    Reference = null,
                    Attachments = null,
                    Content = null
                }.Build());
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception,
                    $"(Log Service) Error in {guild.Id.RawValue} for Guild Member Log - {exception.Message}");
            }
        }
    }
}
