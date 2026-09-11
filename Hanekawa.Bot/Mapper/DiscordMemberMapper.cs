using Disqord;
using Disqord.Gateway;
using Hanekawa.Entities.Discord;

namespace Hanekawa.Bot.Mapper;

internal static class DiscordMemberMapper
{
    internal static DiscordMember ToDiscordMember(IMember member)
    {
        return new DiscordMember
        {
            Guild = new Guild { GuildId = member.GuildId },
            Id = member.Id,
            RoleIds = ToRawRoleIds(member.RoleIds),
            Nickname = member.Nick,
            IsBot = member.IsBot,
            Username = member.Name,
            AvatarUrl = member.GetAvatarUrl(),
            VoiceSessionId = member.GetVoiceState()?.SessionId
        };
    }

    internal static ulong[] ToRawRoleIds(IReadOnlyList<Snowflake> roles)
    {
        var toReturn = new ulong[roles.Count];
        for (var i = 0; i < roles.Count; i++)
        {
            toReturn[i] = roles[i].RawValue;
        }

        return toReturn;
    }
}
