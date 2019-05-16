using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Bot.TypeReaders;
using Hanekawa.Core;
using Hanekawa.Core.Interfaces;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Qmmands;

namespace Hanekawa.Bot.Services.Command
{
    public class CommandHandlingService : INService, IRequired
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _command;
        private readonly ConcurrentDictionary<ulong, HashSet<string>> _prefixes = new ConcurrentDictionary<ulong, HashSet<string>>();

        public CommandHandlingService(DiscordSocketClient client, CommandService command)
        {
            _client = client;
            _command = command;

            using (var db = new DbService())
            {
                foreach (var x in db.GuildConfigs)
                {
                    var hashset = new HashSet<string>(x.Prefix);
                    _prefixes.TryAdd(x.GuildId, hashset);
                }
            }

            _client.MessageReceived += OnMessageReceived;
            _client.LeftGuild += ClientLeft;
            _client.JoinedGuild += ClientJoined;
        }

        public void InitializeAsync()
        {
            var assembly = Assembly.GetEntryAssembly();
            _command.AddModules(assembly);
            _command.AddTypeParser(new EmoteTypeReader());
            _command.AddTypeParser(new GuildUserParser());
            _command.AddTypeParser(new RoleParser());
            _command.AddTypeParser(new TextChannelParser());
            _command.AddTypeParser(new TimeSpanTypeParser());
            _command.AddTypeParser(new VoiceChannelParser());
        }

        public HashSet<string> GetPrefix(ulong id) => _prefixes.GetOrAdd(id, new HashSet<string> {"h."});
        public async Task<bool> AddPrefix(ulong id, string prefix, DbService db)
        {
            var cfg = await db.GetOrCreateGuildConfigAsync(id);
            if (cfg.Prefix.Contains(prefix)) return false;
            cfg.Prefix.Add(prefix);
            var hashset = new HashSet<string>(cfg.Prefix);
            _prefixes.AddOrUpdate(id, new HashSet<string> { "h." }, (key, old) => hashset);
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemovePrefix(ulong id, string prefix, DbService db)
        {
            var cfg = await db.GetOrCreateGuildConfigAsync(id);
            if (!cfg.Prefix.Contains(prefix)) return false;
            cfg.Prefix.Remove(prefix);
            _prefixes.TryRemove(id, out var list);
            list.Remove(prefix);
            _prefixes.TryAdd(id, list);
            await db.SaveChangesAsync();
            return true;
        }

        private async Task OnMessageReceived(SocketMessage rawMsg)
        {
            if (!(rawMsg is SocketUserMessage message)) return;
            if (!(message.Author is SocketGuildUser user)) return;
            if (user.IsBot) return;

            if (!CommandUtilities.HasAnyPrefix(message.Content, GetPrefix(user.Guild.Id), out var prefix, out var output)) return;
            await _command.ExecuteAsync(output, new HanekawaContext(_client, message, user));
        }

        private Task ClientJoined(SocketGuild guild)
        {
            _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var cfg = await db.GetOrCreateGuildConfigAsync(guild);
                    var hashSet = new HashSet<string>(cfg.Prefix);
                    _prefixes.TryAdd(guild.Id, hashSet);
                }
            });
            return Task.CompletedTask;
        }

        private Task ClientLeft(SocketGuild socketGuild)
        {
            _ = Task.Run(() => _prefixes.TryRemove(socketGuild.Id, out _));
            return Task.CompletedTask;
        }
    }
}