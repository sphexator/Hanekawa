using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Hanekawa.Bot.TypeReaders;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Interactive;
using Hanekawa.Shared.Interfaces;
using Humanizer;
using Microsoft.Extensions.Logging;
using Qmmands;
using Module = Qmmands.Module;

namespace Hanekawa.Bot.Services.Command
{
    public class CommandHandlingService : INService, IRequired
    {
        private readonly DiscordSocketClient _client;
        private readonly InternalLogService _log;
        private readonly ColourService _colourService;
        private readonly CommandService _command;
        private readonly InteractiveService _interactive;
        private readonly ConcurrentDictionary<ulong, string> _prefixes = new ConcurrentDictionary<ulong, string>();
        private readonly IServiceProvider _provider;

        public CommandHandlingService(DiscordSocketClient client, CommandService command, IServiceProvider provider,
            InteractiveService interactive, ColourService colourService, InternalLogService log)
        {
            _client = client;
            _command = command;
            _provider = provider;
            _interactive = interactive;
            _colourService = colourService;
            _log = log;

            _client.LeftGuild += ClientLeft;
            _client.JoinedGuild += ClientJoined;
            _client.MessageReceived += message =>
            {
                _ = OnMessageReceived(message);
                return Task.CompletedTask;
            };

            _command.CommandErrored += log =>
            {
                _ = OnCommandError(log);
                return Task.CompletedTask;
            };

            using (var db = new DbService())
            {
                foreach (var x in db.GuildConfigs)
                {
                    _prefixes.TryAdd(x.GuildId, x.Prefix);
                    _colourService.AddOrUpdate(x.GuildId, new Color(x.EmbedColor));
                }
            }
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
            var modules = _command.AddModules(Assembly.GetEntryAssembly());
            _log.LogAction(LogLevel.Information, $"Added {modules.Count} modules");
        }

        public string GetPrefix(ulong id) => _prefixes.GetOrAdd(id, "h.");

        public async Task<bool> AddPrefix(ulong id, string prefix, DbService db)
        {
            var cfg = await db.GetOrCreateGuildConfigAsync(id);
            cfg.Prefix = prefix;
            _prefixes.AddOrUpdate(id, prefix, (key, old) => prefix);
            await db.SaveChangesAsync();
            return true;
        }

        private async Task OnMessageReceived(SocketMessage rawMsg)
        {
            if (!(rawMsg is SocketUserMessage message)) return;
            if (!(message.Author is SocketGuildUser user)) return;
            if (user.IsBot) return;
            string output;
            if (!CommandUtilities.HasPrefix(message.Content, GetPrefix(user.Guild.Id),
                out output) && !message.HasMentionPrefix(user.Guild.CurrentUser, out var prefix, out output)) return;
            var result = await _command.ExecuteAsync(output,
                new HanekawaContext(_client, message, user, _colourService, _interactive), _provider);
            if (!result.IsSuccessful) _log.LogAction(LogLevel.Warning, $"Command: {result}");
        }

        private async Task OnCommandError(CommandErroredEventArgs e)
        {
            _log.LogAction(LogLevel.Error, e.Result.Exception, "Command Error");
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
                    foreach (var x in err.FailedChecks) response.AppendLine($"[{x.Check}]: `{x.Result.Reason}`");
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
                    foreach (var overload in err.FailedOverloads) response.AppendLine($" -> `{overload.Value.Reason}`");
                    break;
                case ParameterChecksFailedResult err:
                    command = err.Parameter.Command;
                    module = err.Parameter.Command.Module;
                    response.AppendLine("The following parameter check(s) failed:");
                    foreach (var (check, error) in err.FailedChecks)
                        response.AppendLine($"[`{check.Parameter.Name}`]: `{error}`");
                    break;
                case ExecutionFailedResult err:
                    switch (err.Exception)
                    {
                        case HttpException _:
                            response.AppendLine("Something went wrong...");
                            break;
                        case HttpRequestException _:
                            response.AppendLine("I am missing the required permissions to perform this action");
                            break;
                        default:
                            response.AppendLine("Something went wrong...");
                            break;
                    }

                    break;
                case CommandNotFoundResult _:
                    var list = new List<Tuple<Qmmands.Command, int>>();
                    var commands = _command.GetAllCommands();
                    foreach (var x in commands)
                        for (var i = 0; i < x.Aliases.Count; i++)
                        {
                            var alias = x.Aliases.ElementAt(i);
                            alias.FuzzyMatch(e.Context.Alias, out var score);
                            list.Add(new Tuple<Qmmands.Command, int>(x, score));
                        }

                    var reCmd = list.OrderByDescending(x => x.Item2).FirstOrDefault();
                    if (reCmd == null) return;
                    _prefixes.TryGetValue(context.Guild.Id, out var prefix);
                    response.AppendLine(
                        $"Did you mean **{reCmd.Item1.Name}** ? (command: {prefix}{reCmd.Item1.FullAliases.FirstOrDefault()})");
                    break;
                default:
                    response.AppendLine("Something went wrong...");
                    break;
            }

            if (response.Length == 0) return;
            await context.Channel.ReplyAsync(response.ToString(), Color.Red);
            await _client.GetGuild(431617676859932704).GetTextChannel(523165903219720232).SendMessageAsync(
                $"Error: {e.Result.Exception.Message}\n" +
                $"{e.Result.Exception.ToString().Truncate(1500)}");
        }

        private Task ClientJoined(SocketGuild guild)
        {
            _ = Task.Run(async () =>
            {
                using var db = new DbService();
                var cfg = await db.GetOrCreateGuildConfigAsync(guild);
                _prefixes.TryAdd(guild.Id, cfg.Prefix);
                _colourService.AddOrUpdate(guild.Id, new Color(cfg.EmbedColor));
            });
            return Task.CompletedTask;
        }

        private Task ClientLeft(SocketGuild guild)
        {
            _ = Task.Run(() =>
            {
                _prefixes.TryRemove(guild.Id, out _);
                _colourService.TryRemove(guild.Id);
            });
            return Task.CompletedTask;
        }
    }
}