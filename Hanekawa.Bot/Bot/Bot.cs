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
using IResult = Qmmands.IResult;

namespace Hanekawa.Bot.Bot;

/// <inheritdoc cref="Hanekawa.Application.Interfaces.IBot" />
public class Bot : DiscordBot, IBot
{
    public Bot(IOptions<DiscordBotConfiguration> options, ILogger<Bot> logger, 
        IServiceProvider services, DiscordClient client) : base(options, logger, services, client)
    { }

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