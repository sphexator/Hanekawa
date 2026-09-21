using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;

namespace Hanekawa.Tests;

internal static class MockDbSetExtensions
{
    public static Mock<DbSet<TEntity>> MockDbSet<TEntity>(this IEnumerable<TEntity> source)
        where TEntity : class
    {
        var collection = source as ICollection<TEntity> ?? source.ToList();
        return collection.BuildMockDbSet<TEntity>();
    }
}
