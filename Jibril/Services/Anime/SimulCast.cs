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
        }

        private Task AnimeAiredAsync(IReadOnlyCollection<AnimeData> collection)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var data = collection.FirstOrDefault();
                    var premiumList = await db.GuildConfigs.Where(x => x.Premium).ToListAsync();
                    foreach (var x in premiumList)
                    {
                        await PostAsync(x, data);
                    }
                }
            });
            return Task.CompletedTask;
        }

        private async Task PostAsync(GuildConfig cfg, AnimeData data)
        {
            if (!cfg.AnimeAirChannel.HasValue) return;
            await _client.GetGuild(cfg.GuildId).GetTextChannel(cfg.AnimeAirChannel.Value).SendMessageAsync(null, false, BuildEmbed(data).Build());
        }

        private static EmbedBuilder BuildEmbed(AnimeData data)
        {
            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder { Name = "New Episode Aired!" },
                Title = $"{data.Title}",
                Url = data.Url,
                Color = Color.Purple,
                Timestamp = new DateTimeOffset(DateTime.UtcNow)
            };
            embed.AddField("Season", data.Season ?? "1", true);
            embed.AddField("Episode", data.Episode, true);
            return embed;
        }
    }
}
