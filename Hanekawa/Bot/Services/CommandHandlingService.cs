using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Core.Interfaces;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;

namespace Hanekawa.Bot.Services
{
    public class CommandHandlingService : INService, IRequired
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _command;
        private readonly ConcurrentDictionary<ulong, string> _prefixes = new ConcurrentDictionary<ulong, string>();

        public CommandHandlingService(DiscordSocketClient client, CommandService command)
        {
            _client = client;
            _command = command;

            using (var db = new DbService())
            {
                foreach (var x in db.GuildConfigs)
                {
                    _prefixes.TryAdd(x.GuildId, x.Prefix);
                }
            }

            _client.MessageReceived += OnMessageReceived;
            _client.LeftGuild += ClientLeft;
            _client.JoinedGuild += ClientJoined;
        }

        public string GetPrefix(ulong id) => _prefixes.GetOrAdd(id, "h.");
        public async Task UpdatePrefix(ulong id, string prefix, DbService db)
        {
            var cfg = await db.GetOrCreateGuildConfigAsync(id);
            cfg.Prefix = prefix;
            _prefixes.AddOrUpdate(id, "h.", (key, old) => prefix);
            await db.SaveChangesAsync();
        }

        private Task ClientJoined(SocketGuild guild)
        {
            _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var cfg = await db.GetOrCreateGuildConfigAsync(guild);
                    _prefixes.TryAdd(guild.Id, cfg.Prefix);
                }
            });
            return Task.CompletedTask;
        }

        private Task ClientLeft(SocketGuild socketGuild)
        {
            _prefixes.TryRemove(socketGuild.Id, out _);
            return Task.CompletedTask;
        }

        private async Task OnMessageReceived(SocketMessage rawMsg)
        {
            if (!(rawMsg.Author is SocketGuildUser user)) return;
            if (user.IsBot) return;
        }
    }
}