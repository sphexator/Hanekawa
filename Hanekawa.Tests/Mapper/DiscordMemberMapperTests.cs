using Disqord;
using Hanekawa.Bot.Mapper;

namespace Hanekawa.Tests.Mapper;

public class DiscordMemberMapperTests
{
    [Fact]
    public void ToRawRoleIds_PreservesSnowflakeOrderAndValues()
    {
        var roles = new[] { new Snowflake(10), new Snowflake(20), new Snowflake(30) };

        var raw = DiscordMemberMapper.ToRawRoleIds(roles);

        Assert.Equal([10ul, 20ul, 30ul], raw);
    }

    [Fact]
    public void ToRawRoleIds_EmptyList_ReturnsEmptyArray()
    {
        Assert.Empty(DiscordMemberMapper.ToRawRoleIds([]));
    }

}
