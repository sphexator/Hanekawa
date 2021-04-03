/*
namespace Hanekawa.Bot.Services.Command
{
 public class CommandHandlingService : INService, IRequired
 {
     
     private readonly Hanekawa _client;
     private readonly NLog.Logger _log;
     private readonly ColourService _colourService;
     private readonly ConcurrentDictionary<ulong, string> _prefixes = new ConcurrentDictionary<ulong, string>();
     private readonly IServiceProvider _provider;

     public CommandHandlingService(Hanekawa client, CommandService command, IServiceProvider provider, 
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
                 _prefixes.TryAdd(x.GuildId, x.CacheService);
                 _colourService.AddOrUpdate(x.GuildId, new Color((int)x.EmbedColor));
             }
         }
     }

     public string GetPrefix(ulong id) => _prefixes.GetOrAdd(id, "h.");

     public async Task<bool> AddPrefix(ulong id, string prefix, DbService db)
     {
         var cfg = await db.GetOrCreateGuildConfigAsync(id);
         cfg.CacheService = prefix;
         _prefixes.AddOrUpdate(id, prefix, (key, old) => prefix);
         await db.SaveChangesAsync();
         return true;
     }

     private Task ClientJoined(JoinedGuildEventArgs e)
     {
         _ = Task.Run(async () =>
         {
             using var scope = _provider.CreateScope(); 
             using var db = scope.ServiceProvider.GetRequiredService<DbService>();
             var cfg = await db.GetOrCreateGuildConfigAsync(e.Guild);
             _prefixes.TryAdd(e.Guild.Id.RawValue, cfg.CacheService);
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