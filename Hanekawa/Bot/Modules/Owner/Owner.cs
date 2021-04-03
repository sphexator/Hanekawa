using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Database;
using Hanekawa.Database.Tables.Administration;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Command.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Modules.Owner
{
    [Name("Owner")]
    [Description("Owner commands for bot overview")]
    [RequireUser(111123736660324352)]
    public class Owner : HanekawaCommandModule
    {
        [Command("mmlol")]
        [RequireGuild(431617676859932704)]
        public async Task ReturnRole()
        {
            try
            {
                await Context.Message.DeleteAsync();
                var role = Context.Guild.GetRole(431621144517279755);
                await Context.Member.GrantRoleAsync(role.Id);
            }
            catch
            {
                await Context.Message.DeleteAsync();
                var roles = await Context.Guild.GetRolesAsync();
                var role = roles.FirstOrDefault(x => x.Id.RawValue == 431621144517279755);
                await Context.Member.GrantRoleAsync(role.Id);
            }
        }

        [Name("Servers")]
        [Command("servers")]
        [Description("List all servers bot is part of")]
        public async Task ServersAsync()
        {
            var servers = new List<string>();
            var totalMembers = 0;
            foreach (var (_, value) in Context.Bot.Guilds.ToList())
            {
                try
                {
                    totalMembers += value.MemberCount;
                    var sb = new StringBuilder();
                    sb.AppendLine($"Server: {value.Name} ({value.Id.RawValue})");
                    sb.AppendLine($"Members: {value.MemberCount}");
                    sb.AppendLine($"Owner: {value.Owner.Mention}");
                    servers.Add(sb.ToString());
                }
                catch { /* IGNORE */}
            }
            await Context.PaginatedReply(servers, Context.Bot.CurrentUser ,null, $"Total Servers: {Context.Bot.Guilds.Count}\n " +
                                                                                                 $"Total Members: {totalMembers}");
        }

        [Name("Blacklist")]
        [Command("blacklist")]
        [Description("Blacklists a server for the bot to join")]
        public async Task BlacklistAsync(ulong guildId, string reason = null)
        {
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var blacklist = await db.Blacklists.FindAsync(guildId);
            if (blacklist == null)
            {
                await db.Blacklists.AddAsync(new Blacklist
                {
                    GuildId = guildId,
                    ResponsibleUser = Context.User.Id.RawValue,
                    Reason = reason
                });
                await db.SaveChangesAsync();
                await Context.ReplyAsync(new LocalEmbedBuilder().Create($"Added blacklist on {guildId}!", Color.Green));
                return;
            }

            blacklist.Unban = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();
            await Context.ReplyAsync(new LocalEmbedBuilder().Create($"Removed blacklist on {guildId}!", Color.Green));
        }

        [Name("Evaluate")]
        [Command("eval")]
        [Description("Evaluates code")]
        public async Task EvaluateAsync([Remainder] string rawCode)
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

                await ReplyAsync(embed: builder.Build());
                return;
            }

            var context = new RoslynCommandContext(Context);
            var result = await script.RunAsync(context);
            sw.Stop();
            await ReplyAsync(result.ReturnValue.ToString());
        }
    }
}
