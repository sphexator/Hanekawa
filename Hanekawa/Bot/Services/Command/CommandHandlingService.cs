using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Bot.TypeReaders;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Interactive;
using Hanekawa.Shared.Interfaces;
using Qmmands;
using Module = Qmmands.Module;

namespace Hanekawa.Bot.Services.Command
{
    public class CommandHandlingService : INService, IRequired
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _command;
        private readonly InteractiveService _interactive;
        private readonly ColourService _colourService;
        private readonly IServiceProvider _provider;
        private readonly ConcurrentDictionary<ulong, HashSet<string>> _prefixes = new ConcurrentDictionary<ulong, HashSet<string>>();

        public CommandHandlingService(DiscordSocketClient client, CommandService command, IServiceProvider provider, InteractiveService interactive, ColourService colourService)
        {
            _client = client;
            _command = command;
            _provider = provider;
            _interactive = interactive;
            _colourService = colourService;

            using (var db = new DbService())
            {
                foreach (var x in db.GuildConfigs)
                {
                    var hashset = new HashSet<string>(x.PrefixList);
                    _prefixes.TryAdd(x.GuildId, hashset);
                }
            }

            _client.LeftGuild += ClientLeft;
            _client.JoinedGuild += ClientJoined;
            _client.MessageReceived += message =>
            {
                _ = OnMessageReceived(message);
                return Task.CompletedTask;
            };

            _command.CommandErrored += log =>
            {
                _ = OnCommandErrored(log);
                return Task.CompletedTask;
            };
        }

        public void InitializeAsync()
        {
            _command.AddTypeParser(new EmoteTypeReader());
            _command.AddTypeParser(new GuildUserParser());
            _command.AddTypeParser(new RoleParser());
            _command.AddTypeParser(new TextChannelParser());
            _command.AddTypeParser(new TimeSpanTypeParser());
            _command.AddTypeParser(new VoiceChannelParser());
            _command.AddTypeParser(new CategoryParser());
            _command.AddModules(Assembly.GetEntryAssembly());
        }

        public HashSet<string> GetPrefix(ulong id) => _prefixes.GetOrAdd(id, new HashSet<string> {"h."});

        public async Task<bool> AddPrefix(ulong id, string prefix, DbService db)
        {
            var cfg = await db.GetOrCreateGuildConfigAsync(id);
            if (cfg.PrefixList.Contains(prefix)) return false;
            cfg.PrefixList.Add(prefix);
            var hashset = new HashSet<string>(cfg.PrefixList);
            _prefixes.AddOrUpdate(id, new HashSet<string> { "h." }, (key, old) => hashset);
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemovePrefix(ulong id, string prefix, DbService db)
        {
            var cfg = await db.GetOrCreateGuildConfigAsync(id);
            if (!cfg.PrefixList.Contains(prefix)) return false;
            cfg.PrefixList.Remove(prefix);
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
            var result = await _command.ExecuteAsync(output, new HanekawaContext(_client, message, user, _colourService, _interactive), _provider);
            if(!result.IsSuccessful) Console.WriteLine("Did not succeed??");
        }

        private async Task OnCommandErrored(CommandErroredEventArgs e)
        {
            if (!(e.Context is HanekawaContext context)) return;
            Qmmands.Command command = null;
            Module module = null;
            var response = new StringBuilder();
            switch (e.Result as IResult)
            {
                case ChecksFailedResult err:
                    command = err.Command;
                    module = err.Module;
                    response.AppendLine("The following check(s) failed:");
                    foreach (var x in err.FailedChecks)
                    {
                        response.AppendLine($"[{x.Check}]: `{x.Result.Reason}`");
                    }
                    break;
                case TypeParseFailedResult err:
                    command = err.Parameter.Command;
                    response.AppendLine(err.Reason);
                    break;
                case ArgumentParseFailedResult err:
                    command = err.Command;
                    response.AppendLine($"The syntax of the command `{command.FullAliases[0]}` was wrong.");
                    break;
                case OverloadsFailedResult err:
                    command = err.FailedOverloads.First().Key;
                    response.AppendLine($"I can't find any valid overload for the command `{command.Name}`.");
                    foreach (var overload in err.FailedOverloads)
                    {
                        response.AppendLine($" -> `{overload.Value.Reason}`");
                    }
                    break;
                case ParameterChecksFailedResult err:
                    command = err.Parameter.Command;
                    module = err.Parameter.Command.Module;
                    response.AppendLine("The following parameter check(s) failed:");
                    foreach (var (check, error) in err.FailedChecks)
                    {
                        response.AppendLine($"[`{check.Parameter.Name}`]: `{error}`");
                    }
                    break;
                case ExecutionFailedResult err:
                    response.AppendLine($"");
                    break;
                case CommandNotFoundResult err:
                    var list = new List<Tuple<Qmmands.Command, int>>();
                    var commands = _command.GetAllCommands();
                    var prefixLength = _prefixes.TryGetValue(context.Guild.Id, out var hashSet);
                    foreach (var x in commands)
                    {
                        for (var i = 0; i < x.Aliases.Count; i++)
                        {
                            var alias = x.Aliases.ElementAt(i);
                            alias.FuzzyMatch(e.Context.Alias, out var score);
                            list.Add(new Tuple<Qmmands.Command, int>(x, score));
                        }
                    }

                    var reCmd = list.OrderByDescending(x => x.Item2).FirstOrDefault();
                    if (reCmd == null) return;
                    response.AppendLine($"Did you mean **{reCmd.Item1.Name}** ? (command: {reCmd.Item1.FullAliases.FirstOrDefault()})");
                    break;
            }

            if (response.Length == 0) return;

        }

        private Task ClientJoined(SocketGuild guild)
        {
            _ = Task.Run(async () =>
            {
                using var db = new DbService();
                var cfg = await db.GetOrCreateGuildConfigAsync(guild);
                var hashSet = new HashSet<string>(cfg.PrefixList);
                _prefixes.TryAdd(guild.Id, hashSet);
            });
            return Task.CompletedTask;
        }

        private Task ClientLeft(SocketGuild guild)
        {
            _ = Task.Run(() => _prefixes.TryRemove(guild.Id, out _));
            return Task.CompletedTask;
        }
    }
}