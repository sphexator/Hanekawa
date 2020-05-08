/*
namespace Hanekawa.Bot.Services.Command
{
 public class CommandHandlingService : INService, IRequired
 {
     
     private readonly DiscordBot _client;
     private readonly InternalLogService _log;
     private readonly ColourService _colourService;
     private readonly ConcurrentDictionary<ulong, string> _prefixes = new ConcurrentDictionary<ulong, string>();
     private readonly IServiceProvider _provider;

     public CommandHandlingService(DiscordBot client, CommandService command, IServiceProvider provider, 
         ColourService colourService, InternalLogService log)
     {
         _client = client;
         _provider = provider;
         _colourService = colourService;
         _log = log;

         _client.LeftGuild += ClientLeft;
         _client.JoinedGuild += ClientJoined;

         _client.CommandExecutionFailed += log =>
         {
             _ = OnCommandError(log);
             return Task.CompletedTask;
         };

         using (var scope = _provider.CreateScope())
         using (var db = scope.ServiceProvider.GetRequiredService<DbService>())
         {
             foreach (var x in db.GuildConfigs)
             {
                 _prefixes.TryAdd(x.GuildId, x.Prefix);
                 _colourService.AddOrUpdate(x.GuildId, new Color((int)x.EmbedColor));
             }
         }
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

     private async Task OnCommandError(CommandExecutionFailedEventArgs e)
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
                 _prefixes.TryGetValue(context.Guild.Id.RawValue, out var prefix);
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

     private Task ClientJoined(JoinedGuildEventArgs e)
     {
         _ = Task.Run(async () =>
         {
             using var scope = _provider.CreateScope(); 
             using var db = scope.ServiceProvider.GetRequiredService<DbService>();
             var cfg = await db.GetOrCreateGuildConfigAsync(e.Guild);
             _prefixes.TryAdd(e.Guild.Id.RawValue, cfg.Prefix);
             _colourService.AddOrUpdate(e.Guild.Id.RawValue, new Color((int)cfg.EmbedColor));
         });
         return Task.CompletedTask;
     }

     private Task ClientLeft(LeftGuildEventArgs e)
     {
         _ = Task.Run(() =>
         {
             _prefixes.TryRemove(e.Guild.Id.RawValue, out _);
             _colourService.TryRemove(e.Guild.Id.RawValue);
         });
         return Task.CompletedTask;
     }
     
 }
}
 */