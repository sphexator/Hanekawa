using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.AnimeSimulCast;
using Hanekawa.AnimeSimulCast.Entity;
using Hanekawa.Core.Interfaces;
using Hanekawa.Database;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Extensions.Embed;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Bot.Services.Anime
{
    public class SimulCastService : INService, IRequired
    {
        private readonly AnimeSimulCastClient _anime;
        private readonly DiscordSocketClient _client;
        private readonly DbService _db;

        public SimulCastService(AnimeSimulCastClient anime, DiscordSocketClient client, DbService db)
        {
            _anime = anime;
            _client = client;
            _db = db;

            _anime.AnimeAired += _anime_AnimeAired;
            _client.Ready += SetupSimulCast;
        }

        private Task SetupSimulCast()
        {
            _anime.StartAsync();
            return Task.CompletedTask;
        }

        private Task _anime_AnimeAired(AnimeData data)
        {
            _ = Task.Run(async () =>
            {
                Console.WriteLine("Anime air announced");
                    var premiumList = await _db.GuildConfigs.Where(x => x.Premium).ToListAsync().ConfigureAwait(false);
                    foreach (var x in premiumList)
                    {
                        await PostAsync(x, data).ConfigureAwait(false);
                        await Task.Delay(5000).ConfigureAwait(false);
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
                .CreateDefault(null, guild)
                .WithAuthor(new EmbedAuthorBuilder { Name = "New Episode Available!" })
                .WithTitle($"{data.Title}")
                .WithUrl(data.Url)
                .WithTimestamp(data.Time);
            return embed;
        }
    }
}