using Disqord;
using Disqord.AuditLogs;
using Disqord.Gateway;
using Disqord.Rest;
using Hanekawa.Entities.Discord;

namespace Hanekawa.Bot.Bot;

/// <summary>
/// A response class that represents a standardized audit log entry
/// </summary>
public record AuditLogResponse
{
    /// <summary>
    /// The ID of the audit log entry
    /// </summary>
    public ulong Id { get; init; }

    /// <summary>
    /// The user who performed the action
    /// </summary>
    public ulong UserId { get; init; }

    /// <summary>
    /// The type of action performed
    /// </summary>
    public AuditLogActionType ActionType { get; init; }

    /// <summary>
    /// The timestamp when the action was performed
    /// </summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// The reason provided for the action (if any)
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// The target ID of the action (if applicable)
    /// </summary>
    public ulong? TargetId { get; set; }

    /// <summary>
    /// Additional data specific to the audit log type (serialized as JSON)
    /// </summary>
    public Dictionary<string, object?> AdditionalData { get; init; } = new();
}

/// <summary>
/// Helper class for retrieving and processing Discord audit logs
/// </summary>
public static class AuditLogHelper
{
    /// <summary>
    /// Retrieves audit logs of a specific type and maps them to a standardized response format
    /// </summary>
    /// <param name="guild">The guild to retrieve audit logs from</param>
    /// <param name="actionType">The specific audit log action type to retrieve</param>
    /// <param name="limit">Maximum number of audit logs to retrieve (optional)</param>
    /// <returns>A list of standardized audit log responses</returns>
    public static async Task<AuditLogResponse[]> GetAuditLogsAsync(CachedGuild guild, AuditLogActionType actionType, int limit = 100)
    {
        var auditLogs = await FetchAuditLogsAsync(guild, actionType, limit);
        return MapAuditLogs(auditLogs);
    }

    /// <summary>
    /// Retrieves all audit logs and maps them to a standardized response format
    /// </summary>
    /// <param name="guild">The guild to retrieve audit logs from</param>
    /// <param name="limit">Maximum number of audit logs to retrieve (optional)</param>
    /// <returns>A list of standardized audit log responses</returns>
    public static async Task<AuditLogResponse[]> GetAllAuditLogsAsync(CachedGuild guild, int limit = 100)
    {
        var auditLogs = await guild.FetchAuditLogsAsync(limit: limit);
        return MapAuditLogs(auditLogs);
    }

    /// <summary>
    /// Maps a collection of audit logs to the standardized response format
    /// </summary>
    private static AuditLogResponse[] MapAuditLogs(IReadOnlyList<IAuditLog> auditLogs)
    {
        var responses = new AuditLogResponse[auditLogs.Count];

        for (var i = 0; i < auditLogs.Count; i++)
        {
            var log = auditLogs[i];
            var response = new AuditLogResponse
            {
                Id = log.Id.RawValue,
                UserId = log.ActorId.Value.RawValue,
                Timestamp = log.CreatedAt(),
                Reason = log.Reason
            };

            // Handle specific audit log types and extract additional data
            ExtractAdditionalData(log, response);

            responses[i] = response;
        }

        return responses;
    }

    /// <summary>
    /// Extracts additional data from specific audit log types
    /// </summary>
    private static void ExtractAdditionalData(IAuditLog log, AuditLogResponse response)
    {
        switch (log)
        {
            case IGuildUpdatedAuditLog guildUpdated:
                response.TargetId = guildUpdated.TargetId;
                break;

            case IChannelCreatedAuditLog channelCreated:
                response.TargetId = channelCreated.TargetId;
                break;

            case IChannelUpdatedAuditLog channelUpdated:
                response.TargetId = channelUpdated.TargetId;
                break;

            case IChannelDeletedAuditLog channelDeleted:
                response.TargetId = channelDeleted.TargetId;
                break;

            case IMemberBannedAuditLog memberBanned:
                response.TargetId = memberBanned.TargetId;
                break;

            case IMemberUnbannedAuditLog memberUnbanned:
                response.TargetId = memberUnbanned.TargetId;
                break;

            case IMemberKickedAuditLog memberKicked:
                response.TargetId = memberKicked.TargetId;
                break;

            case IMemberUpdatedAuditLog memberUpdated:
                response.TargetId = memberUpdated.TargetId;
                break;

            case IRoleCreatedAuditLog roleCreated:
                response.TargetId = roleCreated.TargetId;
                break;

            case IRoleUpdatedAuditLog roleUpdated:
                response.TargetId = roleUpdated.TargetId;
                break;

            case IRoleDeletedAuditLog roleDeleted:
                response.TargetId = roleDeleted.TargetId;
                break;

            case  IMessagesDeletedAuditLog messageDeleted:
                response.TargetId = messageDeleted.TargetId;
                response.AdditionalData["ChannelId"] = messageDeleted.ChannelId;
                break;

            case IMessagesBulkDeletedAuditLog messagesDeleted:
                response.TargetId = messagesDeleted.TargetId;
                response.AdditionalData["ChannelId"] = messagesDeleted.ChannelId;
                response.AdditionalData["MessageCount"] = messagesDeleted.Count;
                break;

            // For any audit log type without a specific handler,
            // try to extract a target ID if the interface implements ITargetedAuditLog
            default:
                    response.TargetId = log.TargetId;
                break;
        }
    }

    /// <summary>
    /// Fetches the appropriate type of audit logs based on the action type
    /// </summary>
    private static async Task<IReadOnlyList<IAuditLog>> FetchAuditLogsAsync(CachedGuild guild, AuditLogActionType actionType, int limit)
    {
        return actionType switch
        {
            AuditLogActionType.GuildUpdated => await guild.FetchAuditLogsAsync<IGuildUpdatedAuditLog>(limit: limit),
            AuditLogActionType.ChannelCreated => await guild.FetchAuditLogsAsync<IChannelCreatedAuditLog>(limit: limit),
            AuditLogActionType.ChannelUpdated => await guild.FetchAuditLogsAsync<IChannelUpdatedAuditLog>(limit: limit),
            AuditLogActionType.ChannelDeleted => await guild.FetchAuditLogsAsync<IChannelDeletedAuditLog>(limit: limit),
            AuditLogActionType.OverwriteCreated => await guild.FetchAuditLogsAsync<IOverwriteCreatedAuditLog>(limit: limit),
            AuditLogActionType.OverwriteUpdated => await guild.FetchAuditLogsAsync<IOverwriteUpdatedAuditLog>(limit: limit),
            AuditLogActionType.OverwriteDeleted => await guild.FetchAuditLogsAsync<IOverwriteDeletedAuditLog>(limit: limit),
            AuditLogActionType.MemberKicked => await guild.FetchAuditLogsAsync<IMemberKickedAuditLog>(limit: limit),
            AuditLogActionType.MembersPruned => await guild.FetchAuditLogsAsync<IMembersPrunedAuditLog>(limit: limit),
            AuditLogActionType.MemberBanned => await guild.FetchAuditLogsAsync<IMemberBannedAuditLog>(limit: limit),
            AuditLogActionType.MemberUnbanned => await guild.FetchAuditLogsAsync<IMemberUnbannedAuditLog>(limit: limit),
            AuditLogActionType.MemberUpdated => await guild.FetchAuditLogsAsync<IMemberUpdatedAuditLog>(limit: limit),
            AuditLogActionType.MemberRolesUpdated => await guild.FetchAuditLogsAsync<IMemberRolesUpdatedAuditLog>(limit: limit),
            AuditLogActionType.MembersMoved => await guild.FetchAuditLogsAsync<IMembersMovedAuditLog>(limit: limit),
            AuditLogActionType.MembersDisconnected => await guild.FetchAuditLogsAsync<IMembersDisconnectedAuditLog>(limit: limit),
            AuditLogActionType.BotAdded => await guild.FetchAuditLogsAsync<IBotAddedAuditLog>(limit: limit),
            AuditLogActionType.RoleCreated => await guild.FetchAuditLogsAsync<IRoleCreatedAuditLog>(limit: limit),
            AuditLogActionType.RoleUpdated => await guild.FetchAuditLogsAsync<IRoleUpdatedAuditLog>(limit: limit),
            AuditLogActionType.RoleDeleted => await guild.FetchAuditLogsAsync<IRoleDeletedAuditLog>(limit: limit),
            AuditLogActionType.InviteCreated => await guild.FetchAuditLogsAsync<IInviteCreatedAuditLog>(limit: limit),
            AuditLogActionType.InviteUpdated => await guild.FetchAuditLogsAsync<IInviteUpdatedAuditLog>(limit: limit),
            AuditLogActionType.InviteDeleted => await guild.FetchAuditLogsAsync<IInviteDeletedAuditLog>(limit: limit),
            AuditLogActionType.WebhookCreated => await guild.FetchAuditLogsAsync<IWebhookCreatedAuditLog>(limit: limit),
            AuditLogActionType.WebhookUpdated => await guild.FetchAuditLogsAsync<IWebhookUpdatedAuditLog>(limit: limit),
            AuditLogActionType.WebhookDeleted => await guild.FetchAuditLogsAsync<IWebhookDeletedAuditLog>(limit: limit),
            AuditLogActionType.EmojiCreated => await guild.FetchAuditLogsAsync<IEmojiCreatedAuditLog>(limit: limit),
            AuditLogActionType.EmojiUpdated => await guild.FetchAuditLogsAsync<IEmojiUpdatedAuditLog>(limit: limit),
            AuditLogActionType.EmojiDeleted => await guild.FetchAuditLogsAsync<IEmojiDeletedAuditLog>(limit: limit),
            AuditLogActionType.MessagesDeleted => await guild.FetchAuditLogsAsync<IMessagesDeletedAuditLog>(limit: limit),
            AuditLogActionType.MessagesBulkDeleted => await guild.FetchAuditLogsAsync<IMessagesBulkDeletedAuditLog>(limit: limit),
            AuditLogActionType.MessagePinned => await guild.FetchAuditLogsAsync<IMessagePinnedAuditLog>(limit: limit),
            AuditLogActionType.MessageUnpinned => await guild.FetchAuditLogsAsync<IMessageUnpinnedAuditLog>(limit: limit),
            AuditLogActionType.IntegrationCreated => await guild.FetchAuditLogsAsync<IIntegrationCreatedAuditLog>(limit: limit),
            AuditLogActionType.IntegrationUpdated => await guild.FetchAuditLogsAsync<IIntegrationUpdatedAuditLog>(limit: limit),
            AuditLogActionType.IntegrationDeleted => await guild.FetchAuditLogsAsync<IIntegrationDeletedAuditLog>(limit: limit),
            AuditLogActionType.StageCreated => await guild.FetchAuditLogsAsync<IStageCreatedAuditLog>(limit: limit),
            AuditLogActionType.StageUpdated => await guild.FetchAuditLogsAsync<IStageUpdatedAuditLog>(limit: limit),
            AuditLogActionType.StageDeleted => await guild.FetchAuditLogsAsync<IStageDeletedAuditLog>(limit: limit),
            AuditLogActionType.StickerCreated => await guild.FetchAuditLogsAsync<IStickerCreatedAuditLog>(limit: limit),
            AuditLogActionType.StickerUpdated => await guild.FetchAuditLogsAsync<IStickerUpdatedAuditLog>(limit: limit),
            AuditLogActionType.StickerDeleted => await guild.FetchAuditLogsAsync<IStickerDeletedAuditLog>(limit: limit),
            AuditLogActionType.GuildEventCreated => await guild.FetchAuditLogsAsync<IGuildEventCreatedAuditLog>(limit: limit),
            AuditLogActionType.GuildEventUpdated => await guild.FetchAuditLogsAsync<IGuildEventUpdatedAuditLog>(limit: limit),
            AuditLogActionType.GuildEventDeleted => await guild.FetchAuditLogsAsync<IGuildEventDeletedAuditLog>(limit: limit),
            AuditLogActionType.ThreadCreate => await guild.FetchAuditLogsAsync<IThreadCreatedAuditLog>(limit: limit),
            AuditLogActionType.ThreadUpdate => await guild.FetchAuditLogsAsync<IThreadUpdatedAuditLog>(limit: limit),
            AuditLogActionType.ThreadDelete => await guild.FetchAuditLogsAsync<IThreadDeletedAuditLog>(limit: limit),
            AuditLogActionType.ApplicationCommandPermissionsUpdate => await guild
                .FetchAuditLogsAsync<IApplicationCommandPermissionsUpdatedAuditLog>(limit: limit),
            AuditLogActionType.AutoModerationRuleCreated => await guild.FetchAuditLogsAsync<IAutoModerationRuleCreatedAuditLog>(limit: limit),
            AuditLogActionType.AutoModerationRuleUpdated => await guild.FetchAuditLogsAsync<IAutoModerationRuleUpdatedAuditLog>(limit: limit),
            AuditLogActionType.AutoModerationRuleDeleted => await guild.FetchAuditLogsAsync<IAutoModerationRuleDeletedAuditLog>(limit: limit),
            AuditLogActionType.AutoModerationMessageBlocked =>
                await guild.FetchAuditLogsAsync<IAutoModerationMessageBlockedAuditLog>(limit: limit),
            _ => throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null)
        };
    }
}