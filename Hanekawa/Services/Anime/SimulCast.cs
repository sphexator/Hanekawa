using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.AnimeSimulCast;
using Hanekawa.Addons.AnimeSimulCast.Entity;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Tables.Config;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Extensions.Embed;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Services.Anime
{
    public class SimulCast : IHanaService, IRequiredService
    {
        private readonly AnimeSimulCastClient _anime;
        private readonly DiscordSocketClient _client;

        public SimulCast(AnimeSimulCastClient anime, DiscordSocketClient client)
        {
            _anime = anime;
            _client = client;

            _anime.AnimeAired += AnimeAiredAsync;
            _client.Ready += StartClient;
            Console.WriteLine("Simulcast service loaded");
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
            Console.WriteLine($"Posting anime event to {guild.Name}");
            await guild.GetTextChannel(cfg.AnimeAirChannel.Value)
                .ReplyAsync(BuildEmbed(data, cfg.GuildId));
        }

        private static EmbedBuilder BuildEmbed(AnimeData data, ulong guild)
        {
            var embed = new EmbedBuilder()
                .CreateDefault(guild)
                .WithAuthor(new EmbedAuthorBuilder {Name = "New Episode Available!"})
                .WithTitle($"{data.Title}")
                .WithUrl(data.Url)
                .WithTimestamp(data.Time);
            return embed;
        }
    }
}