using Discord;
using Discord.Addons.Interactive;
using Discord.Addons.Preconditions;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Data.Variables;
using Jibril.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jibril.Modules.Administration
{
    public class Administration : InteractiveBase
    {
        [Command("prune", RunMode = RunMode.Async)]
        [Alias("Prune")]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireRole(339371670311796736)]
        public async Task ClearMessage([Remainder] int x = 0)
        {
            if (x <= 2000)
            {
                var channel = Context.Channel as ITextChannel;
                var messagesToDelete = await Context.Channel.GetMessagesAsync(x + 1).Flatten();
                await channel.DeleteMessagesAsync(messagesToDelete);
                var embed = EmbedGenerator.DefaultEmbed($"{messagesToDelete} messages deleted!", Colours.OKColour);
                var guild = Context.Guild as SocketGuild;
                await ReplyAndDeleteAsync("", false, embed.Build(), TimeSpan.FromSeconds(15)).ConfigureAwait(false);
            }
            else
            {
                var embed = EmbedGenerator.DefaultEmbed("you cannot delete more than 1000 messages", Colours.FailColour);
                await ReplyAndDeleteAsync("", false, embed.Build(), TimeSpan.FromSeconds(15)).ConfigureAwait(false);
            }
        }

        [Command("Ban", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireRole(339371670311796736)]
        public async Task BanAsync(SocketGuildUser user = null, [Remainder] string reason = "No Reason provided")
        {
            if (user == null) throw new ArgumentException("You must mention a user");
            if (Context.User.Id != user.Guild.OwnerId && (user.Roles.Select(r => r.Position).Max() >= (Context.Guild).Roles.Select(r => r.Position).Max()))
            {
                var failEmbed = EmbedGenerator.DefaultEmbed($"{Context.User.Mention}, you can't ban someone with same or higher role then you.", Colours.FailColour);
                await ReplyAndDeleteAsync("", false, failEmbed.Build(), TimeSpan.FromSeconds(15)).ConfigureAwait(false);
                return;
            }

            var guild = Context.Guild as SocketGuild;
            var embed = EmbedGenerator.DefaultEmbed($"Banned {user.Mention} from {Context.Guild.Name}", Colours.OKColour);

            await guild.AddBanAsync(user, 7, $"{Context.User} - {reason}").ConfigureAwait(false);
            await ReplyAndDeleteAsync("", false, embed.Build(), TimeSpan.FromSeconds(15)).ConfigureAwait(false);
        }

        [Command("Kick", RunMode = RunMode.Async)]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireRole(339371670311796736)]
        public async Task KickAsync(SocketGuildUser user, [Remainder] string reason)
        {
            if (user == null) throw new ArgumentException("You must mention a user");
            if (Context.User.Id != user.Guild.OwnerId && (user.Roles.Select(r => r.Position).Max() >= (Context.Guild).Roles.Select(r => r.Position).Max()))
            {
                var failEmbed = EmbedGenerator.DefaultEmbed($"{Context.User.Mention}, you can't kick someone with same or higher role then you.", Colours.FailColour);
                await ReplyAndDeleteAsync("", false, failEmbed.Build(), TimeSpan.FromSeconds(15));
                return;
            }
            var embed = EmbedGenerator.DefaultEmbed($"Kicked {user.Username} from {Context.Guild.Name}", Colours.OKColour);
            await user.KickAsync().ConfigureAwait(false);
        }

        [Command("mute", RunMode = RunMode.Async)]
        [Alias("Mute", "m")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireRole(339371670311796736)]
        public async Task Mute(SocketGuildUser user)
        {
            if (Context.User.Id != user.Guild.OwnerId && (user.Roles.Select(r => r.Position).Max() >= (Context.Guild).Roles.Select(r => r.Position).Max()))
            {
                var failEmbed = EmbedGenerator.DefaultEmbed($"{Context.User.Mention}, you can't mute someone with same or higher role then you.", Colours.FailColour);
                await ReplyAndDeleteAsync("", false, failEmbed.Build(), TimeSpan.FromSeconds(15));
                return;
            }

            var muteRole = Context.Guild.Roles.FirstOrDefault(r => r.Name == "Mute");
            await user.ModifyAsync(x => x.Mute = true).ConfigureAwait(false);
            try
            {
                await user.AddRoleAsync(muteRole);
            }
            catch
            {
                // Didn't find role
            }
            try
            {
                IMessage[] msgs;
                IMessage lastMessage = null;
                msgs = (await Context.Channel.GetMessagesAsync(50).Flatten()).Where(m => m.Author.Id == user.Id).Take(50).ToArray();
                lastMessage = msgs[msgs.Length - 1];

                var bulkDeletable = new List<IMessage>();
                foreach (var x in msgs)
                {
                    bulkDeletable.Add(x);
                }
                bulkDeletable.Add(Context.Message as IMessage);

                var channel = Context.Channel as ITextChannel;
                await Task.WhenAll(Task.Delay(1000), channel.DeleteMessagesAsync(bulkDeletable)).ConfigureAwait(false);

                var content = $"Action: *Gagged* \n" +
                $"❕ {user.Mention} got *bent*. (**{user.Id}**)\n" +
                $"Moderator: {Context.User.Mention} \n" +
                $"Reason: \n";
                var embed = EmbedGenerator.DefaultEmbed(content, Colours.FailColour);
                var log = Context.Guild.GetChannel(339381104534355970) as ITextChannel;
                await log.SendMessageAsync("", false, embed.Build());
            }
            catch
            {

            }

        }

        [Command("unmute", RunMode = RunMode.Async)]
        [Alias("Unmute", "unm")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireRole(339371670311796736)]
        public async Task Unmute(SocketGuildUser user)
        {
            try
            {
                if (Context.User.Id != user.Guild.OwnerId && (user.Roles.Select(r => r.Position).Max() >= (Context.Guild).Roles.Select(r => r.Position).Max()))
                {
                    // Error message
                    var failEmbed = EmbedGenerator.DefaultEmbed($"{Context.User.Mention}, you can't unmute someone with same or higher role then you.", Colours.FailColour);
                    await ReplyAndDeleteAsync("", false, failEmbed.Build(), TimeSpan.FromSeconds(15));
                    return;
                }

                await user.ModifyAsync(x => x.Mute = false).ConfigureAwait(false);
                var muteRole = Context.Guild.Roles.FirstOrDefault(r => r.Name == "Mute");
                await user.RemoveRoleAsync(muteRole);
                var embed = EmbedGenerator.DefaultEmbed($"unmuted from {user.Mention}.", Colours.OKColour);
                await ReplyAndDeleteAsync("", false, embed.Build(), TimeSpan.FromSeconds(15));
            }
            catch
            {
                // Ignore
            }
        }
    }
}
