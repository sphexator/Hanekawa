using Disqord;
using Disqord.Gateway;
using Hanekawa.Entities;
using Hanekawa.Entities.Discord;

namespace Hanekawa.Bot.Mapper;

internal static class DiscordExtensions
{
    internal static LocalEmbed ToLocalEmbed(this Embed embed)
    {
        var toReturn = new LocalEmbed
        {
            Author = new LocalEmbedAuthor
            {
                Name = embed.Header.Name,
                IconUrl = embed.Header.IconUrl,
                Url = embed.Header.Url
            },
            Title = embed.Title,
            ThumbnailUrl = embed.Icon,
            Color = new Color(embed.Color),
            Description = embed.Content,
            ImageUrl = embed.Attachment,
            Timestamp = embed.Timestamp,
            Footer = new LocalEmbedFooter
            {
                IconUrl = embed.Footer.IconUrl,
                Text = embed.Footer.Text,
            }
        };
        var fields = new List<LocalEmbedField>();
        for (var i = 0; i < embed.Fields.Count; i++)
        {
            var x = embed.Fields[i];
            fields.Add(new LocalEmbedField{ Name = x.Name, Value = x.Value, IsInline = x.IsInline });
        }

        if (fields.Count != 0) toReturn.Fields = fields;
        return toReturn;
    }

    internal static TextChannel ToTextChannel(this TransientInteractionChannel channel) =>
        new()
        {
            Id = channel.Id,
            GuildId = (channel as ITextChannel)?.GuildId ?? 0,
            Name = channel.Name,
            Mention = "<#" + channel.Id + ">",
            IsNsfw = (channel as ITextChannel)?.IsAgeRestricted ?? false
        };

    internal static DiscordMember ToDiscordMember(this IMember member) =>
        new()
        {
            UserId = member.Id,
            Username = member.Name,
            AvatarUrl = member.GetAvatarUrl(),
            Guild = new ()
            {
                Id = member.GetGuild()!.Id,
                Name = member.GetGuild()!.Name,
                IconUrl = member.GetGuild()?.GetIconUrl()!,
                Description = member.GetGuild()?.Description,
                BoostCount = member.GetGuild()?.BoostingMemberCount,
                BoostTier = member.GetGuild()!.BoostTier.GetHashCode(),
                MemberCount = member.GetGuild()!.MemberCount,
                EmoteCount = member.GetGuild()!.Emojis.Count
            },
            Nickname = member.Nick,
            IsBot = member.IsBot,
            RoleIds = member.RoleIds.Select(x => x.RawValue).ToHashSet(),
            VoiceSessionId = member.GetVoiceState()?.SessionId
        };

    internal static Guild ToGuild(this IMember user) =>
        new()
        {
            Id = user.GetGuild()!.Id,
            Name = user.GetGuild()!.Name,
            IconUrl = user.GetGuild()?.GetIconUrl()!,
            Description = user.GetGuild()?.Description,
            BoostCount = user.GetGuild()?.BoostingMemberCount,
            BoostTier = user.GetGuild()!.BoostTier.GetHashCode(),
            MemberCount = user.GetGuild()!.MemberCount,
            EmoteCount = user.GetGuild()!.Emojis.Count
        };
    
    internal static LocalInteractionMessageResponse ToLocalInteractionMessageResponse(this Response<Message> response)
    {
        var toReturn = new LocalInteractionMessageResponse
        {
            Content = response.Data.Content,
            Embeds = new List<LocalEmbed> { response.Data.Embed.ToLocalEmbed() },
            IsTextToSpeech = false,
            AllowedMentions = response.Data.AllowMentions 
                ? LocalAllowedMentions.ExceptEveryone : LocalAllowedMentions.None,
            Components = new List<LocalRowComponent>
            {
                new ()
                {
                    Components = new List<LocalComponent>
                    {
                        
                    }
                }
            },
            IsEphemeral = response.Data.Emphemeral
        };
        return toReturn;
    }
}