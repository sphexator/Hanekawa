using Disqord;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Application;
using Disqord.Bot.Commands.Interaction;
using Disqord.Gateway;
using Hanekawa.Application.Interfaces.Services;
using Hanekawa.Bot.Commands.Checks;
using Hanekawa.Bot.Commands.Metas;
using Hanekawa.Entities;
using Qmmands;

namespace Hanekawa.Bot.Commands.Slash.Activity;

[SlashGroup(SlashGroupName.Activity)]
[RequireModule(ModuleName.Activity)]
public class ActivityCommands : DiscordApplicationGuildModuleBase
{
    [SlashCommand(Metas.ActivityName.Top)]
    [Description("Shows the most active users this week")]
    public async Task<DiscordInteractionResponseCommandResult> TopAsync()
    {
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IActivityService>();
        var leaderboard = await service.GetWeeklyLeaderboardAsync(Context.GuildId);
        if (leaderboard.Count == 0)
            return Response("No activity has been tracked this week yet !");

        var embed = new LocalEmbed()
            .WithTitle("Weekly most active")
            .WithDescription("Ranked by messages sent this week (spam filtered)")
            .WithColor(Color.Gold);

        for (var i = 0; i < leaderboard.Count; i++)
        {
            var entry = leaderboard[i];
            embed.AddField($"#{i + 1} <@{entry.UserId}>", $"Messages: {entry.MessageCount}");
        }

        return Response(new LocalInteractionMessageResponse()
            .WithEmbeds(embed)
            .WithAllowedMentions(LocalAllowedMentions.None));
    }

    [SlashCommand(Metas.ActivityName.RewardPrevious)]
    [Description("Set the role rewarded to the previous week's most active user")]
    [RequireAuthorPermissions(Permissions.ManageGuild)]
    public async Task<DiscordInteractionResponseCommandResult> RewardPreviousAsync(IRole? role = null)
    {
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IActivityService>();
        var response = await service.SetPreviousWeekRoleAsync(Context.GuildId, role?.Id.RawValue);
        return Response(response);
    }

    [SlashCommand(Metas.ActivityName.RewardCurrent)]
    [Description("Set the role held by the current week's most active users")]
    [RequireAuthorPermissions(Permissions.ManageGuild)]
    public async Task<DiscordInteractionResponseCommandResult> RewardCurrentAsync(IRole? role = null,
        [Minimum(1)] int amount = 1)
    {
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IActivityService>();
        var response = await service.SetCurrentWeekRoleAsync(Context.GuildId, role?.Id.RawValue, amount);
        return Response(response);
    }

    [SlashCommand(Metas.ActivityName.Channel)]
    [Description("Set the channel weekly activity announcements are sent to")]
    [RequireAuthorPermissions(Permissions.ManageGuild)]
    public async Task<DiscordInteractionResponseCommandResult> ChannelAsync(IChannel? channel = null)
    {
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IActivityService>();
        if (channel is null)
            return Response(await service.SetAnnouncementChannelAsync(Context.GuildId, null));
        if (channel is not TransientInteractionChannel { Type: ChannelType.Text })
            return Response("The announcement channel must be a text channel !");

        var response = await service.SetAnnouncementChannelAsync(Context.GuildId, channel.Id);
        return Response(response);
    }

    [SlashCommand(Metas.ActivityName.Message)]
    [Description("Set the weekly announcement message. Placeholders: {user}, {count}, {week}")]
    [RequireAuthorPermissions(Permissions.ManageGuild)]
    public async Task<DiscordInteractionResponseCommandResult> MessageAsync(string? message = null)
    {
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IActivityService>();
        var response = await service.SetAnnouncementMessageAsync(Context.GuildId, message);
        return Response(response);
    }
}
