﻿using System.Diagnostics;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Commands;
using Disqord.Rest;
using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Discord;
using Microsoft.Extensions.Options;
using IResult = Qmmands.IResult;

namespace Hanekawa.Bot.Bot;

/// <inheritdoc cref="Hanekawa.Application.Interfaces.IBot" />
public class Bot : DiscordBot, IBot
{
    public Bot(IOptions<DiscordBotConfiguration> options, ILogger<Bot> logger, IServiceProvider services, DiscordClient client) : base(options, logger, services, client) 
    { }

    protected override async ValueTask<IResult> OnBeforeExecuted(IDiscordCommandContext context)
    {
        var start = Stopwatch.GetTimestamp();
        var result = await base.OnBeforeExecuted(context);
        var elapsedTime = Stopwatch.GetElapsedTime(start);
        Serilog.Log.Information("Command {Command} executed in {Elapsed}ms", context.Command?.Name, elapsedTime);
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
            x.RoleIds = ConvertRoles(modifiedRoles);
        });

    private static Snowflake[] ConvertRoles(ulong[] modifiedRoles)
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