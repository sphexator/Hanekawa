using System.Collections.Concurrent;
using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Interfaces.Services;
using Hanekawa.Entities.Activity;
using Hanekawa.Entities.Configs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Application.Services;

/// <inheritdoc />
public class ActivityService(IDbContext db, IBot bot, ILogger<ActivityService> logger) : IActivityService
{
    private static readonly ConcurrentDictionary<ulong, SemaphoreSlim> CurrentWeekRoleSyncLocks = new();

	private const string DefaultAnnouncementMessage =
        "Congratulations {user} for being the most active user last week with {count} messages!";

    /// <inheritdoc />
    public async Task TrackMessageAsync(ulong guildId, ulong userId, DateTimeOffset timestamp,
        CancellationToken cancellationToken = default)
    {
        var weekStart = GetWeekStart(timestamp);
        var activity = await db.WeeklyActivities.FirstOrDefaultAsync(
            x => x.GuildId == guildId && x.UserId == userId && x.WeekStart == weekStart, cancellationToken);
        if (activity is null)
        {
            logger.LogInformation("Starting weekly activity tracking for user {User} in guild {Guild}",
                userId, guildId);
            activity = new GuildActivity
            {
                GuildId = guildId,
                UserId = userId,
                WeekStart = weekStart
            };
            await db.WeeklyActivities.AddAsync(activity, cancellationToken);
        }

        activity.MessageCount++;
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<GuildActivity>> GetWeeklyLeaderboardAsync(ulong guildId, int count = 10,
        CancellationToken cancellationToken = default)
    {
        var weekStart = GetWeekStart(DateTimeOffset.UtcNow);
        return await GetTopUsersAsync(guildId, weekStart, count, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SyncCurrentWeekRolesAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        var gate = CurrentWeekRoleSyncLocks.GetOrAdd(guildId, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        try
        {
            await SyncCurrentWeekRolesCoreAsync(guildId, cancellationToken);
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task SyncCurrentWeekRolesCoreAsync(ulong guildId, CancellationToken cancellationToken)
    {
        var config = await GetConfigAsync(guildId, cancellationToken);
        if (config?.CurrentWeekRoleId is null) return;

        var weekStart = GetWeekStart(DateTimeOffset.UtcNow);
        var topAmount = Math.Max(1, config.CurrentWeekTopAmount);
        var top = (await GetTopUsersAsync(guildId, weekStart, topAmount, cancellationToken))
            .Select(x => x.UserId)
            .ToList();

        var toAdd = top.Except(config.CurrentHolders).ToList();
        var toRemove = config.CurrentHolders.Except(top).ToList();
        if (toAdd.Count == 0 && toRemove.Count == 0) return;

        foreach (var userId in toAdd)
        {
            logger.LogInformation("Assigning current-week activity role {Role} to user {User} in guild {Guild}",
                config.CurrentWeekRoleId.Value, userId, guildId);
            await ApplyRoleChangeAsync(guildId, userId, config.CurrentWeekRoleId.Value, add: true);
        }

        foreach (var userId in toRemove)
        {
            logger.LogInformation("Removing current-week activity role {Role} from user {User} in guild {Guild}",
                config.CurrentWeekRoleId.Value, userId, guildId);
            await ApplyRoleChangeAsync(guildId, userId, config.CurrentWeekRoleId.Value, add: false);
        }

        config.CurrentHolders = top;
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task ProcessWeekRolloversAsync(CancellationToken cancellationToken = default)
    {
        var currentWeek = GetWeekStart(DateTimeOffset.UtcNow);
        var configs = await db.GuildConfigs
            .Include(x => x.ActivityConfig)
            .Where(x => x.ActivityConfig != null &&
                        (x.ActivityConfig.LastProcessedWeekStart == null ||
                         x.ActivityConfig.LastProcessedWeekStart < currentWeek))
            .Select(x => x.ActivityConfig!)
            .ToListAsync(cancellationToken);

        foreach (var config in configs)
        {
            // One guild's Discord failure must not skip the others or drop their watermark update.
            // Only the transition into the current ISO week is processed. Missed earlier weeks are not backfilled.
            try
            {
                await ProcessRolloverAsync(config, currentWeek, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Failed to process activity week rollover for guild {Guild}", config.GuildId);
            }
        }

        if (configs.Count > 0) await db.SaveChangesAsync(cancellationToken);
    }

    private async Task ProcessRolloverAsync(ActivityConfig config, DateOnly currentWeek,
        CancellationToken cancellationToken)
    {
        var previousWeek = currentWeek.AddDays(-7);
        logger.LogInformation("Processing activity week rollover for guild {Guild} (week {Week})",
            config.GuildId, previousWeek);

        var winner = (await GetTopUsersAsync(config.GuildId, previousWeek, 1, cancellationToken))
            .FirstOrDefault();

        if (winner is not null && config.PreviousWeekRoleId.HasValue)
        {
            if (config.PreviousWeekHolderId.HasValue && config.PreviousWeekHolderId.Value != winner.UserId)
            {
                await ApplyRoleChangeAsync(config.GuildId, config.PreviousWeekHolderId.Value,
                    config.PreviousWeekRoleId.Value, add: false);
            }

            if (config.PreviousWeekHolderId != winner.UserId)
            {
                await ApplyRoleChangeAsync(config.GuildId, winner.UserId, config.PreviousWeekRoleId.Value, add: true);
            }

            config.PreviousWeekHolderId = winner.UserId;
        }

        if (winner is not null && config.AnnouncementChannelId.HasValue)
        {
            var message = (config.AnnouncementMessage ?? DefaultAnnouncementMessage)
                .Replace("{user}", $"<@{winner.UserId}>")
                .Replace("{count}", winner.MessageCount.ToString())
                .Replace("{week}", previousWeek.ToString("yyyy-MM-dd"));
            await bot.SendMessageAsync(config.AnnouncementChannelId.Value, message);
        }

        // New week: clear the current-week role from last week's holders so it can be earned again.
        if (config is { CurrentWeekRoleId: not null, CurrentHolders.Count: > 0 })
        {
            foreach (var holder in config.CurrentHolders)
            {
                await ApplyRoleChangeAsync(config.GuildId, holder, config.CurrentWeekRoleId.Value, add: false);
            }

            config.CurrentHolders = [];
        }

        config.LastProcessedWeekStart = currentWeek;
    }

    /// <inheritdoc />
    public async Task<string> SetPreviousWeekRoleAsync(ulong guildId, ulong? roleId,
        CancellationToken cancellationToken = default)
    {
        var config = await GetOrCreateConfigAsync(guildId, cancellationToken);
        if (config.PreviousWeekRoleId.HasValue &&
            config.PreviousWeekRoleId != roleId &&
            config.PreviousWeekHolderId.HasValue)
        {
            await ApplyRoleChangeAsync(config.GuildId, config.PreviousWeekHolderId.Value,
                config.PreviousWeekRoleId.Value, add: false);
            config.PreviousWeekHolderId = null;
        }

        config.PreviousWeekRoleId = roleId;
        await db.SaveChangesAsync(cancellationToken);
        return roleId.HasValue
            ? $"Previous week's most active user will now receive <@&{roleId}> !"
            : "Previous week's most active role reward disabled !";
    }

    /// <inheritdoc />
    public async Task<string> SetCurrentWeekRoleAsync(ulong guildId, ulong? roleId, int topAmount,
        CancellationToken cancellationToken = default)
    {
        var config = await GetOrCreateConfigAsync(guildId, cancellationToken);
        if (config.CurrentWeekRoleId.HasValue &&
            config.CurrentWeekRoleId != roleId &&
            config.CurrentHolders.Count > 0)
        {
            await RemoveCurrentWeekRoleFromHoldersAsync(config, config.CurrentWeekRoleId.Value,
                cancellationToken);
        }

        config.CurrentWeekRoleId = roleId;
        config.CurrentWeekTopAmount = Math.Max(1, topAmount);
        await db.SaveChangesAsync(cancellationToken);
        return roleId.HasValue
            ? $"Current week's top {config.CurrentWeekTopAmount} most active will now hold <@&{roleId}> !"
            : "Current week's most active role reward disabled !";
    }

    /// <inheritdoc />
    public async Task<string> SetAnnouncementChannelAsync(ulong guildId, ulong? channelId,
        CancellationToken cancellationToken = default)
    {
        var config = await GetOrCreateConfigAsync(guildId, cancellationToken);
        config.AnnouncementChannelId = channelId;
        await db.SaveChangesAsync(cancellationToken);
        return channelId.HasValue
            ? $"Weekly activity announcements will be sent to <#{channelId}> !"
            : "Weekly activity announcements disabled !";
    }

    /// <inheritdoc />
    public async Task<string> SetAnnouncementMessageAsync(ulong guildId, string? message,
        CancellationToken cancellationToken = default)
    {
        var config = await GetOrCreateConfigAsync(guildId, cancellationToken);
        config.AnnouncementMessage = string.IsNullOrWhiteSpace(message) ? null : message.Trim();
        await db.SaveChangesAsync(cancellationToken);
        return config.AnnouncementMessage is null
            ? "Weekly announcement message reset to default !"
            : "Weekly announcement message updated !";
    }

    private async Task<List<GuildActivity>> GetTopUsersAsync(ulong guildId, DateOnly weekStart, int count,
        CancellationToken cancellationToken)
        => await db.WeeklyActivities
            .Where(x => x.GuildId == guildId && x.WeekStart == weekStart)
            .OrderByDescending(x => x.MessageCount)
            .Take(count)
            .ToListAsync(cancellationToken);

    private async Task<ActivityConfig?> GetConfigAsync(ulong guildId, CancellationToken cancellationToken)
    {
        var config = await db.GuildConfigs
            .Include(x => x.ActivityConfig)
            .FirstOrDefaultAsync(x => x.GuildId == guildId, cancellationToken);
        return config?.ActivityConfig;
    }

    private async Task RemoveCurrentWeekRoleFromHoldersAsync(ActivityConfig config, ulong roleId,
        CancellationToken cancellationToken)
    {
        foreach (var userId in config.CurrentHolders)
        {
            logger.LogInformation(
                "Removing previous current-week activity role {Role} from user {User} in guild {Guild}",
                roleId, userId, config.GuildId);
            await ApplyRoleChangeAsync(config.GuildId, userId, roleId, add: false);
        }

        config.CurrentHolders = [];
    }

    /// <summary>
    /// Applies an activity-role grant or revoke. A member who left, or a role that was
    /// deleted, makes Discord return 404 (Unknown Member / Unknown Role). That is permanent
    /// for this holder: failing the whole sync would skip everyone after them, never save
    /// <see cref="ActivityConfig.CurrentHolders"/> or <see cref="ActivityConfig.LastProcessedWeekStart"/>,
    /// and retry the weekly announcement every 15 minutes.
    /// </summary>
    private async Task ApplyRoleChangeAsync(ulong guildId, ulong userId, ulong roleId, bool add)
    {
        try
        {
            if (add)
                await bot.AddRoleAsync(guildId, userId, roleId);
            else
                await bot.RemoveRoleAsync(guildId, userId, roleId);
        }
        catch (Exception ex) when (ex is not OperationCanceledException && IsMissingRoleTarget(ex))
        {
            logger.LogWarning(ex,
                "Skipped activity role {Action} of {Role} for user {User} in guild {Guild} because the member or role no longer exists",
                add ? "grant" : "revoke", roleId, userId, guildId);
        }
    }

    private static bool IsMissingRoleTarget(Exception ex)
    {
        for (var current = ex; current is not null; current = current.InnerException)
        {
            if (current.Message.Contains("Unknown Member", StringComparison.Ordinal)
                || current.Message.Contains("Unknown Role", StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    private async Task<ActivityConfig> GetOrCreateConfigAsync(ulong guildId,
        CancellationToken cancellationToken)
    {
        var config = await db.GuildConfigs
            .Include(x => x.ActivityConfig)
            .FirstOrDefaultAsync(x => x.GuildId == guildId, cancellationToken);
        if (config is null)
        {
            config = new GuildConfig { GuildId = guildId, ActivityConfig = new ActivityConfig(guildId) };
            await db.GuildConfigs.AddAsync(config, cancellationToken);
        }

        config.ActivityConfig ??= new ActivityConfig(guildId);
        return config.ActivityConfig;
    }

    /// <summary>
    /// Monday (ISO 8601) of the week the timestamp falls in, in UTC.
    /// </summary>
    internal static DateOnly GetWeekStart(DateTimeOffset timestamp)
    {
        var date = DateOnly.FromDateTime(timestamp.UtcDateTime);
        var diff = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return date.AddDays(-diff);
    }
}
