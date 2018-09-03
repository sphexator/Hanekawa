using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;

namespace Hanekawa.Modules.Administration
{
    public class Roles : InteractiveBase
    {
        [Command("lewd")]
        [Alias("nsfw")]
        public async Task AssignLewd()
        {
            var channel = Context.Channel.Id.ToString();
            if (channel == "339380254097539072" || channel == "339383206669320192")
                try
                {
                    var user = Context.User as SocketGuildUser;
                    var altUser = Context.User as IGuildUser;
                    var lewdRole = Context.Guild.Roles.First(x => x.Id == 339711429211062273);
                    var roleCheck = altUser.RoleIds.Contains(lewdRole.Id);
                    if (roleCheck == false)
                    {
                        await Context.Message.DeleteAsync().ConfigureAwait(false);
                        await user.AddRoleAsync(lewdRole).ConfigureAwait(false);
                        var embed = new EmbedBuilder();
                        embed.WithColor(Color.Purple);
                        embed.Description = "Assigned lewd role";
                        await ReplyAndDeleteAsync("", false, embed.Build(), TimeSpan.FromSeconds(5))
                            .ConfigureAwait(false);
                    }
                    else if (roleCheck)
                    {
                        await Context.Message.DeleteAsync().ConfigureAwait(false);
                        await user.RemoveRoleAsync(lewdRole).ConfigureAwait(false);
                        var embed = new EmbedBuilder();
                        embed.WithColor(Color.Purple);
                        embed.Description = "Removed lewd role";
                        await ReplyAndDeleteAsync("", false, embed.Build(), TimeSpan.FromSeconds(5))
                            .ConfigureAwait(false);
                    }
                }
                catch
                {
                    await ReplyAndDeleteAsync(":Thinking:", false, null, TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                }
        }

        [Command("picdump")]
        [Alias("pic", "pdump", "picd")]
        public async Task AssignPicDump()
        {
            var channel = Context.Channel.Id.ToString();
            if (channel == "339380254097539072" || channel == "339383206669320192")
                try
                {
                    var user = Context.User as SocketGuildUser;
                    var altUser = Context.User as IGuildUser;
                    var picdump = Context.Guild.Roles.FirstOrDefault(r => r.Name == "picdump");
                    var roleCheck = altUser.RoleIds.Contains(picdump.Id);
                    if (roleCheck == false)
                    {
                        await Context.Message.DeleteAsync().ConfigureAwait(false);
                        await user.AddRoleAsync(picdump).ConfigureAwait(false);
                        var embed = new EmbedBuilder();
                        embed.WithColor(Color.Purple);
                        embed.Description = "Assigned picdump role";
                        await ReplyAndDeleteAsync("", false, embed.Build(), TimeSpan.FromSeconds(5))
                            .ConfigureAwait(false);
                    }
                    else if (roleCheck)
                    {
                        await Context.Message.DeleteAsync();
                        await user.RemoveRoleAsync(picdump);
                        var embed = new EmbedBuilder();
                        embed.WithColor(Color.Purple);
                        embed.Description = "Removed picdump role";
                        await ReplyAndDeleteAsync("", false, embed.Build(), TimeSpan.FromSeconds(5))
                            .ConfigureAwait(false);
                    }
                }
                catch
                {
                    await Context.Channel.SendMessageAsync("Walla").ConfigureAwait(false);
                }
        }

        [Command("japanese")]
        public async Task AssignJapanese()
        {
            var channel = Context.Channel.Id.ToString();
            if (channel == "339380254097539072" || channel == "339383206669320192")
                try
                {
                    var user = Context.User as SocketGuildUser;
                    var altUser = Context.User as IGuildUser;
                    var japanese = Context.Guild.Roles.FirstOrDefault(r => r.Name == "Japanese");
                    var roleCheck = altUser.RoleIds.Contains(japanese.Id);
                    if (roleCheck == false)
                    {
                        await Context.Message.DeleteAsync().ConfigureAwait(false);
                        await user.AddRoleAsync(japanese).ConfigureAwait(false);
                        var embed = new EmbedBuilder();
                        embed.WithColor(Color.Purple);
                        embed.Description = "Assigned Japanese role";
                        await ReplyAndDeleteAsync("", false, embed.Build(), TimeSpan.FromSeconds(5))
                            .ConfigureAwait(false);
                    }
                    else if (roleCheck)
                    {
                        await Context.Message.DeleteAsync();
                        await user.RemoveRoleAsync(japanese);
                        var embed = new EmbedBuilder();
                        embed.WithColor(Color.Purple);
                        embed.Description = "Removed Japanese role";
                        await ReplyAndDeleteAsync("", false, embed.Build(), TimeSpan.FromSeconds(5))
                            .ConfigureAwait(false);
                    }
                }
                catch
                {
                    // Ignore
                }
        }
    }
}
