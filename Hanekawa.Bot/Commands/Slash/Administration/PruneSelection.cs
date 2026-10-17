namespace Hanekawa.Bot.Commands.Slash.Administration;

/// <summary>
/// Chooses message ids Discord will accept for a bulk delete.
/// </summary>
internal static class PruneSelection
{
    /// <summary>
    /// Discord rejects a bulk delete when any id is missing or older than 14 days,
    /// and the whole request fails.
    /// </summary>
    internal static readonly TimeSpan MaxAge = TimeSpan.FromDays(14);

    internal static ulong[] SelectDeletable(
        IEnumerable<(ulong Id, DateTimeOffset CreatedAt)> messages,
        DateTimeOffset utcNow)
    {
        var cutoff = utcNow - MaxAge;
        var ids = new List<ulong>();
        foreach (var (id, createdAt) in messages)
        {
            if (id == 0 || createdAt < cutoff)
                continue;

            ids.Add(id);
        }

        return ids.ToArray();
    }
}
