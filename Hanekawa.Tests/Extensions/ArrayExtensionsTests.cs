using Hanekawa.Extensions;

namespace Hanekawa.Tests.Extensions;

public class ArrayExtensionsTests
{
    [Fact]
    public void Add_ReturnsNewArrayWithItem_WithoutMutatingOriginal()
    {
        var original = new ulong[] { 1, 2, 3 };

        var result = original.Add(4UL);

        Assert.Equal(new ulong[] { 1, 2, 3 }, original);
        Assert.Equal(new ulong[] { 1, 2, 3, 4 }, result);
    }

    [Fact]
    public void Remove_ReturnsNewArrayWithoutItem()
    {
        var original = new ulong[] { 1, 2, 3 };

        var result = original.Remove(2UL);

        Assert.Equal(new ulong[] { 1, 2, 3 }, original);
        Assert.Equal(new ulong[] { 1, 3 }, result);
    }
}
