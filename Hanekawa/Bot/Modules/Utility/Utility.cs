using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Bot.Services;
using Hanekawa.Shared.Command;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Qmmands;

namespace Hanekawa.Bot.Modules.Utility
{
    public class Utility : HanekawaCommandModule
    {
        private readonly HttpClient _httpClient;
        public Utility(HttpClient httpClient) => _httpClient = httpClient;

        [Name("Add Emote")]
        [Command("emote")]
        [Description("Add(s) emotes to the server by its name")]
        [RequireMemberGuildPermissions(Permission.ManageEmojis)]
        public async Task AddEmotesAsync(params LocalCustomEmoji[] emote)
        {
            var list = new StringBuilder();
            foreach (var x in emote)
            {
                try
                {
                    var stream = new MemoryStream();
                    var stream1 = await _httpClient.GetStreamAsync(x.GetUrl(256));
                    await stream1.FlushAsync();
                    await stream1.CopyToAsync(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    try
                    {
                        var result = await Context.Guild.CreateEmojiAsync(stream, x.Name);
                        list.Append($"{result} ");
                    }
                    catch
                    {
                        var result = await Context.Guild.CreateEmojiAsync(stream, "ToBeRenamed");
                        list.Append($"{result} (rename)");
                    }
                }
                catch (Exception e)
                {
                    Context.ServiceProvider.GetRequiredService<InternalLogService>().LogAction(LogLevel.Error, e, $"{e.Message}\n{e.StackTrace}");
                    // Ignore
                }
            }
            await ReplyAsync($"Added emotes\n {list}");
        }
    }
}
