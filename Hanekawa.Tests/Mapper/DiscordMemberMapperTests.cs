using Disqord;
using Hanekawa.Bot.Mapper;

namespace Hanekawa.Tests.Mapper;

public class DiscordMemberMapperTests
{
    [Fact]
    public void ToRawRoleIds_PreservesSnowflakeOrderAndValues()
    {
        var roles = new[] { new Snowflake(100UL), new Snowflake(200UL), new Snowflake(300UL) };

        var raw = DiscordMemberMapper.ToRawRoleIds(roles);

        Assert.Equal([100UL, 200UL, 300UL], raw);
    }

    [Fact]
    public void ToRawRoleIds_EmptyRoleList_ReturnsEmptyArray()
    {
        IReadOnlyList<Snowflake> roles = Array.Empty<Snowflake>();

        var raw = DiscordMemberMapper.ToRawRoleIds(roles);

        Assert.Empty(raw);
    }
}
