using Discord;
using Discord.WebSocket;
using Hanekawa.AnimeSimulCast;
using Hanekawa.AnimeSimulCast.Entity;
using Hanekawa.Services.Entities;
using Hanekawa.Services.Entities.Tables;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hanekawa.Services.Anime
{
    public class SimulCast
    {
        private readonly DiscordSocketClient _client;
        private readonly AnimeSimulCastClient _anime;
        public SimulCast(AnimeSimulCastClient anime, DiscordSocketClient client)
        {
            _anime = anime;
            _client = client;

            _anime.AnimeAired += AnimeAiredAsync;
            _client.Ready += StartClient;
        }

        private Task StartClient()
        {
            _anime.StartAsync();
            return Task.CompletedTask;
        }

        private Task AnimeAiredAsync(AnimeData data)
        {
            var _ = Task.Run(async () =>
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
            if (!cfg.AnimeAirChannel.HasValue) return;
            var guild = _client.GetGuild(cfg.GuildId);
            Console.WriteLine($"Posting to {guild.Name}");
            await guild.GetTextChannel(cfg.AnimeAirChannel.Value).SendMessageAsync(null, false, BuildEmbed(data).Build());
        }

        private static EmbedBuilder BuildEmbed(AnimeData data)
        {
            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder { Name = "New Episode Aired!" },
                Title = $"{data.Title}",
                Url = data.Url,
                Color = Color.Purple,
                Timestamp = data.Time
            };
            embed.AddField("Season", data.Season ?? "1", true);
            embed.AddField("Episode", data.Episode, true);
            return embed;
        }
    }
}
