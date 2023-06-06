using System.Diagnostics;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Commands;
using Disqord.Gateway;
using Disqord.Rest;
using Hanekawa.Application.Handlers.Metrics;
using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Discord;
using MediatR;
using Microsoft.Extensions.Options;
using Prometheus.Client;
using IResult = Qmmands.IResult;

namespace Hanekawa.Bot.Bot;

/// <inheritdoc cref="Hanekawa.Application.Interfaces.IBot" />
public class Bot : DiscordBot, IBot
{
    public Bot(IOptions<DiscordBotConfiguration> options, ILogger<Bot> logger, 
        IServiceProvider services, DiscordClient client) : base(options, logger, services, client)
    { }

    protected override ValueTask<bool> OnAfterExecuted(IDiscordCommandContext context, IResult result)
    {
        Services.GetRequiredService<IMetricFactory>().CreateCounter("hanekawa_commands_executed_total", 
            "Total number of commands executed", "command", "module").WithLabels(
            context.Command?.Name ?? "Unknown", context.Command?.Module.Name ?? "Unknown").Inc();
        return base.OnAfterExecuted(context, result);
    }

    protected override async ValueTask<IResult> OnBeforeExecuted(IDiscordCommandContext context)
    {
        var start = Stopwatch.GetTimestamp();
        var result = await base.OnBeforeExecuted(context);
        var elapsedTime = Stopwatch.GetElapsedTime(start);
        Serilog.Log.Information("Command {Command} executed in {Elapsed}ms", 
            context.Command?.Name, elapsedTime);

        if(context.GuildId is not null)
            await Services.GetRequiredService<IMediator>().Send(new CommandMetric(context.GuildId.Value, 
                context.Author.Id, $"{context.Command?.Module.Name ?? "Unknown"}-{context.Command?.Name ?? "Unknown"}", 
                DateTimeOffset.UtcNow));
        
        return result;
    }

    /// <inheritdoc />
    public async Task BanAsync(ulong guildId, ulong userId, int days, string reason)
        => await this.CreateBanAsync(guildId, userId, reason, days, new DefaultRestRequestOptions { Reason = reason });
    
    /// <inheritdoc />
    public async Task UnbanAsync(ulong guildId, ulong userId, string reason)
        => await this.DeleteBanAsync(guildId, userId, new DefaultRestRequestOptions { Reason = reason });
    
    /// <inheritdoc />
    public async Task KickAsync(ulong guildId, ulong userId, string reason)
        => await this.KickMemberAsync(guildId, userId, new DefaultRestRequestOptions { Reason = reason});
    
    /// <inheritdoc />
    public async Task MuteAsync(ulong guildId, ulong userId, string reason, TimeSpan duration) 
        => await this.ModifyMemberAsync(guildId, userId, x =>
        {
            x.TimedOutUntil = DateTimeOffset.UtcNow.Add(duration);
        }, new DefaultRestRequestOptions{ Reason = reason } );
    
    /// <inheritdoc />
    public async Task UnmuteAsync(ulong guildId, ulong userId, string reason) 
        => await this.ModifyMemberAsync(guildId, userId, x =>
        {
            x.TimedOutUntil = null;
        }, new DefaultRestRequestOptions{ Reason = reason } );
    
    /// <inheritdoc />
    public async Task AddRoleAsync(ulong guildId, ulong userId, ulong roleId) 
        => await this.GrantRoleAsync(guildId, userId, roleId);
    
    /// <inheritdoc />
    public async Task RemoveRoleAsync(ulong guildId, ulong userId, ulong roleId) 
        => await this.RevokeRoleAsync(guildId, userId, roleId);
    
    /// <inheritdoc />
    public async Task ModifyRolesAsync(DiscordMember member, ulong[] modifiedRoles) 
        => await this.ModifyMemberAsync(member.GuildId, member.UserId, x =>
        {
            x.RoleIds = ConvertToSnowflake(modifiedRoles);
        });
    
    /// <inheritdoc />
    public ulong? GetChannel(ulong guildId, ulong channelId) 
        => this.GetGuild(guildId)!.GetChannel(channelId)!.Id;

    /// <inheritdoc />
    public async Task PruneMessagesAsync(ulong guildId, ulong channelId, ulong[] messageIds) 
        => await (this.GetGuild(guildId)!.GetChannel(channelId) as ITextChannel)
            !.DeleteMessagesAsync(ConvertToSnowflake(messageIds));

    /// <inheritdoc />
    public async Task SendMessageAsync(ulong channelId, string message, Attachment? attachment = null)
    {
        var localMsg = new LocalMessage()
            .WithContent(message)
            .WithAllowedMentions(LocalAllowedMentions.None);
        if (attachment is not null) localMsg.WithAttachments(new LocalAttachment(attachment.Stream, attachment.FileName));
        await this.SendMessageAsync(channelId, localMsg);
    }

    /// <inheritdoc />
    public async Task SendMessageAsync(ulong channelId, Embed embedMessage, Attachment? attachment = null)
    {
        var embed = new LocalEmbed()
        {
            Author = new LocalEmbedAuthor()
            {
                Name = embedMessage.Header.Item2,
                IconUrl = embedMessage.Header.Item1,
                Url = embedMessage.Header.Item3
            },
            Title = embedMessage.Title,
            ThumbnailUrl = embedMessage.Icon,
            Color = new Color(embedMessage.Color),
            Description = embedMessage.Content,
            ImageUrl = embedMessage.Attachment,
            Timestamp = embedMessage.Timestamp,
            Footer = new LocalEmbedFooter
            {
                IconUrl = embedMessage.Footer.Item1,
                Text = embedMessage.Footer.Item2
            }
        };
        var fields = new List<LocalEmbedField>();
        for (var i = 0; i < embedMessage.Fields.Count; i++)
        {
            var x = embedMessage.Fields[i];
            fields.Add(new LocalEmbedField{ Name = x.Item1, Value = x.Item2, IsInline = x.Item3 });
        }

        if (fields.Count != 0) embed.Fields = fields;
        var localMsg = new LocalMessage()
            .WithEmbeds(embed)
            .WithAllowedMentions(LocalAllowedMentions.None);
        if (attachment is not null) localMsg.WithAttachments(new LocalAttachment(attachment.Stream, attachment.FileName));
        await this.SendMessageAsync(channelId, localMsg);
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