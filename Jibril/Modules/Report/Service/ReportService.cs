using Discord.WebSocket;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Jibril.Data.Variables;

namespace Jibril.Modules.Report.Service
{
    public class ReportService
    {
        private readonly DiscordSocketClient _client;
        public ReportService(DiscordSocketClient client)
        {
            _client = client;
        }

        private async Task SendReport(EmbedBuilder embed)
        {
            var ch = _client.GetGuild(339370914724446208).GetTextChannel(439475837419388943);
            await ch.SendMessageAsync(null, false, embed.Build());
        }

        private EmbedBuilder CreateEmbedBuilder(IUser user, SocketCommandContext context, string content)
        {
            var author = new EmbedAuthorBuilder
            {
                Name = user.Username
            };
            var footer = new EmbedFooterBuilder
            {
                Text = $""
            };
            var embed = new EmbedBuilder
            {
                Author = author,
                Color = new Color(Colours.DefaultColour),
                Title = "Report",
                Description = content
            };
            return embed;
        }
    }
}
