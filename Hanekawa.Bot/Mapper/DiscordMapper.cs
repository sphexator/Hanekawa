using Disqord;
using Disqord.Extensions.Interactivity.Menus.Paged;
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
            Title = embed.Title ?? string.Empty,
            ThumbnailUrl = embed.Icon ?? string.Empty,
            Color = new Color(embed.Color),
            Description = embed.Content ?? string.Empty,
            ImageUrl = embed.Attachment ?? string.Empty,
            Timestamp = embed.Timestamp
        };
        if (embed.Header is not null)
        {
            toReturn.Author = new LocalEmbedAuthor
            {
                Name = embed.Header.Name,
                IconUrl = embed.Header.IconUrl,
                Url = embed.Header.Url
            };
        }
        if (embed.Footer is not null)
        {
            toReturn.Footer= new LocalEmbedFooter
            {
                IconUrl = embed.Footer.IconUrl,
                Text = embed.Footer.Text,
            };
        }
        var fields = new List<LocalEmbedField>();
        for (var i = 0; i < embed.Fields.Count; i++)
        {
            var x = embed.Fields[i];
            fields.Add(new LocalEmbedField
            {
                Name = x.Name,
                Value = x.Value,
                IsInline = x.IsInline
            });
        }

        if (fields is not { Count: 0 }) toReturn.Fields = fields;
        return toReturn;
    }

    internal static Embed ToEmbed(this IEmbed embed)
    {
        var toReturn = new Embed
        {
            Header = new EmbedHeader(embed.Author!.Name, (embed.Author.IconUrl ?? null)!, (embed.Author.Url ?? null)!),
            Title = embed.Title,
            Icon = embed.Thumbnail!.Url,
            Color = embed.Color!.Value.RawValue,
            Content = embed.Description!,
            Attachment = embed.Image!.Url,
            Timestamp = embed.Timestamp!.Value,
            Footer = new EmbedFooter((embed.Footer!.Text), (embed.Footer.IconUrl ?? null)!)
        };
        if (embed.Author is not null)
        {
            toReturn.Header = new EmbedHeader(embed.Author.Name,
                embed.Author.IconUrl ?? string.Empty,
                embed.Author.Url ?? string.Empty);
        }
        if (embed.Footer is not null)
        {
            toReturn.Footer = new EmbedFooter(embed.Footer.IconUrl ?? string.Empty, embed.Footer.Text);
        }
        return toReturn;
    }

    internal static TextChannel ToTextChannel(this TransientInteractionChannel channel) =>
        new()
        {
            Id = channel.Id,
            Category = (channel as ITextChannel)?.CategoryId ?? null,
            GuildId = (channel as ITextChannel)?.GuildId ?? null,
            Name = channel.Name,
            Mention = "<#" + channel.Id + ">",
            IsNsfw = (channel as ITextChannel)?.IsAgeRestricted ?? true
        };

    internal static DiscordMember ToDiscordMember(this IMember? member)
    {
        if (member is null) return new DiscordMember();
        return new DiscordMember
        {
            Id = member.Id,
            Username = member.Name,
            AvatarUrl = member.GetAvatarUrl(),
            GuildId = member.GetGuild()!.Id,
            Guild = new Guild
            {
                GuildId = member.GetGuild()!.Id,
                Name = member.GetGuild()!.Name,
                IconUrl = member.GetGuild()?.GetIconUrl()!,
                Description = member.GetGuild()?.Description,
                BoostCount = member.GetGuild()?.BoostingMemberCount,
                BoostTier = member.GetGuild()!.BoostTier.GetHashCode(),
                MemberCount = member.GetGuild()!.MemberCount,
                Emotes = member.GetGuild()!.Emojis
                    .Select(e => new Emote
                    {
                        Id = e.Key.RawValue,
                        Name = e.Value.Name,
                        Format = e.Value.GetReactionFormat(),
                        IsAnimated = e.Value.IsAnimated,
                        IsAvailable = e.Value.IsAvailable,
                        IsManaged = e.Value.IsManaged
                    }).ToArray()
            },
            Nickname = member.Nick,
            IsBot = member.IsBot,
            RoleIds = member.RoleIds.Select(x => x.RawValue).ToArray(),
            VoiceSessionId = member.GetVoiceState()?.SessionId
        };
    }

    internal static Guild ToGuild(this IMember user) =>
        new()
        {
            GuildId = user.GetGuild()!.Id,
            Name = user.GetGuild()!.Name,
            IconUrl = user.GetGuild()?.GetIconUrl()!,
            Description = user.GetGuild()?.Description,
            BoostCount = user.GetGuild()?.BoostingMemberCount,
            BoostTier = user.GetGuild()!.BoostTier.GetHashCode(),
            MemberCount = user.GetGuild()!.MemberCount,
            Emotes = user.GetGuild()!.Emojis
                .Select(e => new Emote
                {
                    Id = e.Key.RawValue,
                    Name = e.Value.Name,
                    Format = e.Value.GetReactionFormat(),
                    IsAnimated = e.Value.IsAnimated,
                    IsAvailable = e.Value.IsAvailable,
                    IsManaged = e.Value.IsManaged
                }).ToArray()
        };

    internal static LocalInteractionMessageResponse ToLocalInteractionMessageResponse(this Response<Message> response)
    {
        var local = new LocalInteractionMessageResponse
        {
            Content = response.Value.Content,
            IsTextToSpeech = false,
            AllowedMentions = response.Value.AllowMentions
                ? LocalAllowedMentions.ExceptEveryone : LocalAllowedMentions.None,
            Components = new List<LocalRowComponent>
            {
                new()
                {
                    Components = new List<LocalComponent>()
                }
            },
            IsEphemeral = response.Value.Emphemeral
        };

        // Text-only command replies (ban/warn/club/etc.) leave Embed null.
        // Mapping it unconditionally NullRef'd after the action already succeeded.
        if (response.Value.Embed is not null)
        {
            local.Embeds = new List<LocalEmbed> { response.Value.Embed.ToLocalEmbed() };
        }

        return local;
    }

    internal static Page[] ToPages(this Response<Pagination<Message>> list)
    {
        var items = list.Value.Items;
        var pages = new Page[items.Length];
        for (var i = 0; i < items.Length; i++)
        {
            pages[i] = new Page
            {
                Content = items[i].Content
            };
        }
        return pages;
    }
}