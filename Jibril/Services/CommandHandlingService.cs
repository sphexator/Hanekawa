using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Extensions;
using Hanekawa.Services.Entities;

namespace Hanekawa.Services
{
    public class CommandHandlingService
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        private IServiceProvider _provider;

        private ConcurrentDictionary<ulong, string> Prefix { get; }
            = new ConcurrentDictionary<ulong, string>();

        public CommandHandlingService(IServiceProvider provider, DiscordSocketClient discord, CommandService commands)
        {
            _discord = discord;
            _commands = commands;
            _provider = provider;

            _discord.MessageReceived += MessageRecieved;

            using (var db = new DbService())
            {
                foreach (var x in db.GuildConfigs)
                {
                    Prefix.GetOrAdd(x.GuildId, x.Prefix);
                }
            }
        }

        public async Task UpdatePrefixAsync(SocketGuild guild, string prefix)
        {
            using (var db = new DbService())
            {
                Prefix.AddOrUpdate(guild.Id, prefix, (key, old) => prefix);
                var cfg = await db.GetOrCreateGuildConfig(guild);
                cfg.Prefix = prefix;
                await db.SaveChangesAsync();
            }
        }

        public async Task InitializeAsync(IServiceProvider provider)
        {
            _provider = provider;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }

        private async Task MessageRecieved(SocketMessage rawMessage)
        {
            if (rawMessage.Author.IsBot) return;
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;
            var argPos = 0;
            var prefix = message.Author is SocketGuildUser user ? Prefix.GetOrAdd(user.Guild.Id, ".") : ".";
            if (!message.HasStringPrefix(prefix, ref argPos) && !message.HasMentionPrefix(_discord.CurrentUser, ref argPos)) return;
            
            var context = new SocketCommandContext(_discord, message);
            await _commands.ExecuteAsync(context, argPos, _provider);
        }
    }
}