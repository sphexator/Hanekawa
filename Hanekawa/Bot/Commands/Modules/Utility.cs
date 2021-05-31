using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity;
using Disqord.Gateway;
using Disqord.Rest;
using Hanekawa.Bot.Commands.Preconditions;
using Hanekawa.Bot.Service.Cache;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Entities.Color;
using Hanekawa.Extensions;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Qmmands;

namespace Hanekawa.Bot.Commands.Modules
{
    [Name("Utility")]
    [Description("Utility Commands")]
    public class Utility : HanekawaCommandModule
    {
        [Name("Emote")]
        [Description("Uploads the given emote(s) to the server")]
        [Command("emote")]
        [RequireAuthorGuildPermissions(Permission.ManageEmojis)]
        public async Task<DiscordCommandResult> EmoteAsync(params ICustomEmoji[] emojis)
        {
            var list = new StringBuilder();
            var client = Context.Services.GetRequiredService<IHttpClientFactory>().CreateClient();
            foreach (var x in emojis)
            {
                try
                {
                    var stream = new MemoryStream();
                    var stream1 = await client.GetStreamAsync(x.GetEmoteUrl());
                    await stream1.FlushAsync();
                    await stream1.CopyToAsync(stream);
                    stream.Position = 0;
                    try
                    {
                        var result = await Context.Guild.CreateEmojiAsync(x.Name, stream);
                        list.Append($"{result} ");
                    }
                    catch(Exception e)
                    {
                        LogManager.GetCurrentClassLogger().Log(LogLevel.Error, e, $"{e.Message}\n{e.StackTrace}");
                        var result = await Context.Guild.CreateEmojiAsync("ToBeRenamed", stream);
                        list.Append($"{result} (rename)");
                    }
                }
                catch (Exception e)
                {
                    LogManager.GetCurrentClassLogger().Log(LogLevel.Error, e, $"{e.Message}\n{e.StackTrace}");
                    // Ignore
                }
            }

            return Reply($"Added emotes\n {list}",
                Context.Services.GetRequiredService<CacheService>().GetColor(Context.GuildId));
        }

        [Name("Emote")]
        [Description("Uploads the given URL to the server with the given name")]
        [Command("emote")]
        [RequireAuthorGuildPermissions(Permission.ManageEmojis)]
        public async Task<DiscordCommandResult> EmoteAsync(string url, string name)
        {
            if (!(url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) |
                  url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)))
                return Reply("First parameter needs to be a link", HanaBaseColor.Bad());
            
            var client = Context.Services.GetRequiredService<IHttpClientFactory>().CreateClient();
            var stream = await client.GetStreamAsync(url);
            var result = stream.ToEditable(1);
            result.Position = 0;
            if (result.GetKnownFileType() == FileType.Unknown) return Reply("Unknown file type", HanaBaseColor.Bad());
            try
            {
                var emote = await Context.Guild.CreateEmojiAsync(name, result);
                return Reply($"Added {emote}",
                    Context.Services.GetRequiredService<CacheService>().GetColor(Context.GuildId));
            }
            catch(Exception e)
            {
                LogManager.GetCurrentClassLogger().Log(LogLevel.Error, e, $"{e.Message}\n{e.StackTrace}");
                var emote = await Context.Guild.CreateEmojiAsync("ToBeRenamed", result);
                return Reply($"Added {emote}",
                    Context.Services.GetRequiredService<CacheService>().GetColor(Context.GuildId));
            }
        }

        [Name("Move")]
        [Description(
            "Move a user to your voice channel, only user that's been in VC the longest can use this command")]
        [Command("move")]
        [RequiredChannel]
        [RequireVoiceState]
        public async Task<DiscordCommandResult> MoveAsync(IMember user)
        {
            var userVoiceState = user.GetVoiceState();
            if (userVoiceState is not {ChannelId: { }})
                return Reply($"{user.DisplayName()} needs to be in a VC first!", HanaBaseColor.Bad());
            var vcState = Context.Author.GetVoiceState();
            var voiceStates = Context.Guild.GetVoiceStates().Where(x =>
                vcState.ChannelId != null && x.Value.ChannelId.HasValue &&
                x.Value.ChannelId.Value == vcState.ChannelId.Value).ToArray();
            
            if (voiceStates.Length == 0)
            {
                await user.ModifyAsync(x =>
                {
                    if (vcState.ChannelId != null) x.VoiceChannelId = vcState.ChannelId.Value;
                });
                return Reply($"Moved {user.DisplayName()} to {Context.Guild.GetChannel(vcState.ChannelId.Value).Name} !",
                    HanaBaseColor.Ok());
            }
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            Database.Tables.Account.Account movePerm = null;
            foreach (var xPair in voiceStates)
            {
                var userData = await db.GetOrCreateUserData(Context.GuildId, xPair.Key);
                if (movePerm == null || movePerm.VoiceExpTime <= userData.VoiceExpTime) movePerm = userData;
            }
            
            if (movePerm != null && Context.Author.Id != movePerm.UserId)
                return Reply(
                    $"Ask {(await Context.Guild.GetOrFetchMemberAsync(movePerm.UserId)).DisplayName()} to move!",
                    HanaBaseColor.Bad());
            await Response(
                $"Do you wish  to be moved into {Context.Guild.GetChannel(vcState.ChannelId.Value).Name}? (y/n)");
            var response = await Context.WaitForMessageAsync(e =>
                e.Channel.Id == Context.ChannelId && e.Member.Id == user.Id &&
                (e.Message.Content.ToLower() == "y" || e.Message.Content.ToLower() == "yes" ||
                 e.Message.Content.ToLower() == "n" || e.Message.Content.ToLower() == "no"));
            if (response == null) return Response($"Timed out...", HanaBaseColor.Bad(), LocalMentionsBuilder.None);
            await user.ModifyAsync(x => x.VoiceChannelId = vcState.ChannelId.Value);
            return Reply($"Moved {user.DisplayName()} to {Context.Guild.GetChannel(vcState.ChannelId.Value).Name} !",
                HanaBaseColor.Ok());
        }

        [Name("Avatar")]
        [Command("avatar", "pfp")]
        [Description("Sends a embeded message containing the profile picture of user provided, if empty it'll return your own.")]
        [RequireBotGuildPermissions(Permission.EmbedLinks | Permission.SendMessages)]
        [RequiredChannel]
        public async ValueTask<DiscordResponseCommandResult> AvatarAsync(IMember user = null)
        {
            user ??= Context.Author;
            var restUser = await user.GetGuild().GetOrFetchMemberAsync(user.Id);
            var embed = new LocalEmbedBuilder
            {
                Author = new LocalEmbedAuthorBuilder {Name = $"{user}"},
                Title = "Avatar URL",
                Url = restUser.GetAvatarUrl(),
                ImageUrl = restUser.GetAvatarUrl(),
                Color = Context.Services.GetRequiredService<CacheService>().GetColor(user.GuildId)
            };
            return Reply(embed);
        }

        [Name("Server Info")]
        [Command("serverinfo")]
        [Description("Obtain information about the server")]
        [RequiredChannel]
        public DiscordCommandResult ServerInfo()
        {
            var sb = new StringBuilder();
            var channels = Context.Guild.GetChannels();
            sb.AppendLine($"Category: {channels.Count(x => x.Value is ICategoryChannel)}");
            sb.AppendLine($"Voice: {channels.Count(x => x.Value is IVoiceChannel)}");
            sb.AppendLine($"Text: {channels.Count(x => x.Value is ITextChannel)}");
            return Reply(new LocalEmbedBuilder
            {
                Author = new LocalEmbedAuthorBuilder{ Name = Context.Guild.Name, IconUrl = Context.Guild.GetIconUrl() },
                Color = Context.Services.GetRequiredService<CacheService>().GetColor(Context.GuildId),
                ThumbnailUrl = Context.Guild.GetIconUrl(),
                Title = $"ID: {Context.GuildId}",
                Fields = new List<LocalEmbedFieldBuilder>
                {
                    new () { Name = $"Verification Level", Value = Context.Guild.VerificationLevel.ToString(), IsInline = false},
                    new () { Name = "Region", Value = Context.Guild.VoiceRegion, IsInline = true},
                    new () { Name = "Members", Value = $"{Context.Guild.MemberCount}"},
                    new () { Name = "Channels", Value = sb.ToString()},
                    new () { Name = "Server Owner", Value = $"{Context.Guild.GetMember(Context.Guild.OwnerId)} ({Context.Guild.OwnerId})"},
                    new () { Name = "Created On", Value = $"{Context.Guild.CreatedAt.DayOfWeek}, {Context.Guild.CreatedAt.MonthOfYear()} {Context.Guild.CreatedAt.Day} @ {Context.Guild.CreatedAt.TimeOfDay}"},
                    new () { Name = "Roles", Value = $"{Context.Guild.Roles.Count}"},
                    new () { Name = "Server Boosts", Value = $"Level: {Context.Guild.BoostTier}\nAmount of boosts: {Context.Guild.BoostingMemberCount ?? 0}"}
                }
            });
        }
    }
}