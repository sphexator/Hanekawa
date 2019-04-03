using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Entities.Interfaces;
using Qmmands;

namespace Hanekawa.Bot.Services
{
    public class CommandHandlingService : INService, IRequired
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _command;
        private readonly ConcurrentDictionary<ulong, string> _prefixes = new ConcurrentDictionary<ulong, string>();
        private readonly DbService _db;

        public CommandHandlingService(DiscordSocketClient client, CommandService command, DbService db)
        {
            _client = client;
            _command = command;
            _db = db;

            foreach (var x in _db.GuildConfigs)
            {
                _prefixes.TryAdd(x.GuildId, x.Prefix);
            }

            _client.MessageReceived += OnMessageReceived;
            _client.LeftGuild += ClientLeft;
            _client.JoinedGuild += ClientJoined;
        }

        public string GetPrefix(ulong id) => _prefixes.GetOrAdd(id, "h.");
        public async Task UpdatePrefix(ulong id, string prefix)
        {
            var cfg = await _db.GetOrCreateGuildConfigAsync(id);
            cfg.Prefix = prefix;
            _prefixes.AddOrUpdate(id, "h.", (key, old) => prefix);
            await _db.SaveChangesAsync();
        }

        private Task ClientJoined(SocketGuild guild)
        {
            _ = Task.Run(async () =>
            {
                var cfg = await _db.GetOrCreateGuildConfigAsync(guild);
                _prefixes.TryAdd(guild.Id, cfg.Prefix);
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
            if (!CommandUtilities.HasPrefix(rawMsg.Content, GetPrefix(user.Guild.Id), out var output)) return;

            var result = await _command.ExecuteAsync(output, null, null);
            if (result is FailedResult failedResult)
            {
                await rawMsg.Channel.SendMessageAsync(null, false, new EmbedBuilder
                {
                    Color = Color.Red,
                    Description = "An error occured."
                }.Build());
            }
        }
    }
}