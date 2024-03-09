using System.Diagnostics;
using System.Diagnostics.Metrics;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Commands;
using Disqord.Gateway;
using Disqord.Rest;
using Disqord.Rest.Pagination;
using Hanekawa.Application.Handlers.Services.Metrics;
using Hanekawa.Application.Interfaces;
using Hanekawa.Bot.Mapper;
using Hanekawa.Entities;
using Hanekawa.Entities.Discord;
using MediatR;
using Microsoft.Extensions.Options;
using Prometheus.Client;
using IResult = Qmmands.IResult;

namespace Hanekawa.Bot.Bot;

/// <inheritdoc cref="Hanekawa.Application.Interfaces.IBot" />
public sealed class Bot : DiscordBot, IBot
{
    private readonly Counter<long> _commandsExecutedTotal;

    public Bot(IOptions<DiscordBotConfiguration> options, ILogger<Bot> logger,
            IServiceProvider services, DiscordClient client)
            : base(options, logger, services, client)
    {
        var meter = services.GetRequiredService<IMeterFactory>().Create(MeterName.DiscordCommands);
        _commandsExecutedTotal = meter.CreateCounter<long>(MeterName.DiscordCommands + ".Total");
    }
    
    protected override ValueTask<bool> OnAfterExecuted(IDiscordCommandContext context, IResult result)
    {
        _commandsExecutedTotal.Add(1);
        return base.OnAfterExecuted(context, result);
    }

    protected override async ValueTask<IResult> OnBeforeExecuted(IDiscordCommandContext context)
    {
        var start = Stopwatch.GetTimestamp();
        var result = await base.OnBeforeExecuted(context);
        var elapsedTime = Stopwatch.GetElapsedTime(start);
        Serilog.Log.Information("Command {Command} executed in {Elapsed}ms", 
            context.Command?.Name, elapsedTime);
        return result;
    }

    /// <inheritdoc />
    public Task BanAsync(ulong guildId, ulong userId, int days, string reason)
        => this.CreateBanAsync(guildId, userId, reason, days, new DefaultRestRequestOptions { Reason = reason });
    
    /// <inheritdoc />
    public Task UnbanAsync(ulong guildId, ulong userId, string reason)
        => this.DeleteBanAsync(guildId, userId, new DefaultRestRequestOptions { Reason = reason });
    
    /// <inheritdoc />
    public Task KickAsync(ulong guildId, ulong userId, string reason)
        => this.KickMemberAsync(guildId, userId, new DefaultRestRequestOptions { Reason = reason});
    
    /// <inheritdoc />
    public Task MuteAsync(ulong guildId, ulong userId, string reason, TimeSpan duration) 
        => this.ModifyMemberAsync(guildId, userId, x =>
        {
            x.TimedOutUntil = DateTimeOffset.UtcNow.Add(duration);
        }, new DefaultRestRequestOptions{ Reason = reason } );
    
    /// <inheritdoc />
    public Task UnmuteAsync(ulong guildId, ulong userId, string reason) 
        => this.ModifyMemberAsync(guildId, userId, x =>
        {
            x.TimedOutUntil = null;
        }, new DefaultRestRequestOptions{ Reason = reason } );
    
    /// <inheritdoc />
    public Task AddRoleAsync(ulong guildId, ulong userId, ulong roleId) 
        => this.GrantRoleAsync(guildId, userId, roleId);
    
    /// <inheritdoc />
    public Task RemoveRoleAsync(ulong guildId, ulong userId, ulong roleId) 
        => this.RevokeRoleAsync(guildId, userId, roleId);
    
    /// <inheritdoc />
    public Task ModifyRolesAsync(DiscordMember member, ulong[] modifiedRoles) 
        => this.ModifyMemberAsync(member.Guild.Id, member.Id, x =>
        {
            x.RoleIds = ConvertToSnowflake(modifiedRoles);
        });
    
    /// <inheritdoc />
    public ulong? GetChannel(ulong guildId, ulong channelId) 
        => this.GetGuild(guildId)!.GetChannel(channelId)!.Id;

    /// <inheritdoc />
    public Task PruneMessagesAsync(ulong guildId, ulong channelId, ulong[] messageIds) 
        => (this.GetGuild(guildId)!.GetChannel(channelId) as ITextChannel)
            !.DeleteMessagesAsync(ConvertToSnowflake(messageIds));

    /// <inheritdoc />
    public Task DeleteMessageAsync(ulong guildId, ulong channelId, ulong messageId) 
        => (this.GetGuild(guildId)?.GetChannel(channelId) as ITextChannel)
            !.DeleteMessageAsync(messageId);

    /// <inheritdoc />
    public async Task<RestMessage> SendMessageAsync(ulong channelId, string message, Attachment? attachment = null)
    {
        var localMsg = new LocalMessage()
            .WithContent(message)
            .WithAllowedMentions(LocalAllowedMentions.None);
        if (attachment is not null) localMsg.WithAttachments(new LocalAttachment(attachment.Stream, attachment.FileName));
        var result = await this.SendMessageAsync(channelId, localMsg).ConfigureAwait(false);
        return new ()
        {
            Id = result.Id,
            ChannelId = result.ChannelId,
            Content = result.Content,
            AuthorId = result.Author.Id, 
            Attachment = result.Attachments.FirstOrDefault()?.Url,
            Embed = result.Embeds[0].ToEmbed()
        };
    }

    /// <inheritdoc />
    public async Task SendMessageAsync(ulong channelId, Embed embedMessage, Attachment? attachment = null)
    {
        var localMsg = new LocalMessage()
            .WithEmbeds(embedMessage.ToLocalEmbed())
            .WithAllowedMentions(LocalAllowedMentions.None);
        if (attachment is not null) localMsg.WithAttachments(new LocalAttachment(attachment.Stream, attachment.FileName));
        await this.SendMessageAsync(channelId, localMsg).ConfigureAwait(false);
    }

    public async Task GetAuditLogAsync(ulong guildId)
    {
        var auditLogs = this.GetGuild(guildId)!.EnumerateAuditLogs(5);
        var result = await auditLogs.FlattenAsync().ConfigureAwait(false);
    }

    private static Snowflake[] ConvertToSnowflake(ulong[] modifiedRoles)
    {
        var result = new Snowflake[modifiedRoles.Length];
        var span = modifiedRoles.AsSpan();
        for (var i = 0; i < span.Length; i++)
        {
            result[i] = span[i];
        }

        return result;
    }
}