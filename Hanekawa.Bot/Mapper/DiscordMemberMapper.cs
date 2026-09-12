using Disqord;

namespace Hanekawa.Bot.Mapper;

internal static class DiscordMemberMapper
{
    internal static ulong[] ToRawRoleIds(IReadOnlyList<Snowflake> roles)
    {
        var toReturn = new ulong[roles.Count];
        for (var i = 0; i < roles.Count; i++)
            toReturn[i] = roles[i].RawValue;
        return toReturn;
    }
}
