using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.AnimeSimulCast;
using Hanekawa.AnimeSimulCast.Entity;
using Hanekawa.Database;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Services.Anime
{
    public class SimulCastService : INService, IRequired
    {
        private readonly AnimeSimulCastClient _anime;
        private readonly DiscordSocketClient _client;
        private readonly InternalLogService _log;

        public SimulCastService(AnimeSimulCastClient anime, DiscordSocketClient client, InternalLogService log)
        {
            _anime = anime;
            _client = client;
            _log = log;

            _anime.AnimeAired += AnimeAired;
            _client.Ready += SetupSimulCast;
        }

        private Task SetupSimulCast()
        {
            _anime.Start();
            return Task.CompletedTask;
        }

        private Task AnimeAired(AnimeData data)
        {
            _ = Task.Run(async () =>
            {
                Console.WriteLine("Anime air announced");
                using (var db = new DbService())
                {
                    var premiumList = await db.GuildConfigs.Where(x => x.Premium).ToListAsync().ConfigureAwait(false);
                    foreach (var x in premiumList)
                    {
                        await PostAsync(x, data).ConfigureAwait(false);
                        await Task.Delay(5000).ConfigureAwait(false);
                    }
                }
            });
            return Task.CompletedTask;
        }

        private async Task PostAsync(GuildConfig cfg, AnimeData data)
        {
            try
            {
                if (!cfg.AnimeAirChannel.HasValue) return;
                var guild = _client.GetGuild(cfg.GuildId);
                Console.WriteLine($"Posting anime event to {guild.Name}");
                await guild.GetTextChannel(cfg.AnimeAirChannel.Value)
                    .ReplyAsync(BuildEmbed(data, cfg.GuildId));
            }
            catch (Exception e)
            {
                _log.LogAction(LogLevel.Error, e, $"(Anime Simulcast) Error for {cfg.GuildId} - {e.Message}");
            }
        }

        private static EmbedBuilder BuildEmbed(AnimeData data, ulong guild)
        {
            var embed = new EmbedBuilder()
                .CreateDefault(null, guild)
                .WithAuthor(new EmbedAuthorBuilder {Name = "New Episode Available!"})
                .WithTitle($"{data.Title}")
                .WithUrl(data.Url)
                .WithTimestamp(data.Time);
            return embed;
        }
    }
}