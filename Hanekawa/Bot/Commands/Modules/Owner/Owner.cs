using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Rest;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Database;
using Hanekawa.Database.Tables.Administration;
using Hanekawa.Entities.Color;
using Hanekawa.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Commands.Modules.Owner
{
    [Name("Owner")]
    [Description("Owner commands")]
    [RequireAuthor(111123736660324352)]
    public class Owner : HanekawaCommandModule
    {
        [Command("mmlol")]
        [RequireGuild(431617676859932704)]
        public async Task ReturnRole()
        {
            try
            {
                await Context.Message.DeleteAsync();
                Context.Guild.Roles.TryGetValue(431621144517279755, out var role);
                await Context.Author.GrantRoleAsync(role.Id);
            }
            catch
            {
                await Context.Message.DeleteAsync();
                var roles = await Context.Guild.FetchRolesAsync();
                var role = roles.FirstOrDefault(x => x.Id == 431621144517279755);
                await Context.Author.GrantRoleAsync(role.Id);
            }
        }
        
        [Name("Servers")]
        [Command("servers")]
        [Description("List all servers bot is part of")]
        public DiscordMenuCommandResult ServersAsync()
        {
            var servers = new List<string>();
            var totalMembers = 0;
            var guilds = Context.Bot.GetGuilds();
            foreach (var (_, value) in Context.Bot.GetGuilds())
            {
                try
                {
                    totalMembers += value.MemberCount;
                    var sb = new StringBuilder();
                    sb.AppendLine($"Server: {value.Name} ({value.Id.RawValue})");
                    sb.AppendLine(value.MaxMemberCount != null
                        ? $"Members: {value.MaxMemberCount.Value}"
                        : $"Members: {value.MemberCount}");
                    sb.AppendLine($"Owner: {Context.Bot.GetMember(value.Id, value.OwnerId)}");
                    servers.Add(sb.ToString());
                }
                catch { /* IGNORE */}
            }

            return Pages(servers.PaginationBuilder(
                Context.Services.GetRequiredService<CacheService>().GetColor(Context.GuildId),
                Context.Bot.CurrentUser.GetAvatarUrl(), $"Server Count: {guilds.Count} | Member Count: {totalMembers}"));
        }
        
                [Name("Blacklist")]
        [Command("blacklist")]
        [Description("Blacklists a server for the bot to join")]
        public async Task<DiscordCommandResult> BlacklistAsync(ulong guildId, string reason = null)
        {
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var blacklist = await db.Blacklists.FindAsync(guildId);
            if (blacklist == null)
            {
                await db.Blacklists.AddAsync(new Blacklist
                {
                    GuildId = guildId,
                    ResponsibleUser = Context.Author.Id.RawValue,
                    Reason = reason
                });
                await db.SaveChangesAsync();
                return Reply($"Added blacklist on {guildId}!", HanaBaseColor.Ok());
            }

            blacklist.Unban = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            return Reply($"Removed blacklist on {guildId}!", HanaBaseColor.Ok());
        }

        [Name("Evaluate")]
        [Command("eval")]
        [Description("Evaluates code")]
        public async Task<DiscordResponseCommandResult> EvaluateAsync([Remainder] string rawCode)
        {
            var code = rawCode.GetCode();
            var sw = Stopwatch.StartNew();
            var script = CSharpScript.Create(code, RoslynExtensions.RoslynScriptOptions, typeof(RoslynCommandContext));
            var diagnostics = script.Compile();
            var compilationTime = sw.ElapsedMilliseconds;

            if (diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))
            {
                var builder = new LocalEmbedBuilder
                {
                    Title = "Compilation Failure",
                    Color = Color.Red,
                    Description = $"Compilation took {compilationTime}ms but failed due to..."
                };
                foreach (var diagnostic in diagnostics)
                {
                    var message = diagnostic.GetMessage();
                    builder.AddField(diagnostic.Id,
                        message.Substring(0, Math.Min(500, message.Length)));
                }

                return Reply(builder);
            }

            var context = new RoslynCommandContext(Context);
            var result = await script.RunAsync(context);
            sw.Stop();
            return Reply(result.ReturnValue.ToString());
        }
    }
}