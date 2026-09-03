using Disqord;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Application;
using Disqord.Bot.Commands.Interaction;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Gateway;
using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Interfaces.Commands;
using Hanekawa.Bot.Commands.Checks;
using Hanekawa.Bot.Commands.Metas;
using Hanekawa.Bot.Mapper;
using Hanekawa.Entities;
using Hanekawa.Entities.Configs;
using Hanekawa.Localize;
using Qmmands;
using IResult = Qmmands.IResult;

namespace Hanekawa.Bot.Commands.Slash.Setting;

[SlashGroup(SlashGroupName.Stream)]
[RequireAuthorPermissions(Permissions.ManageGuild)]
[RequireModule(ModuleName.Streaming)]
public class StreamCommands(IMetrics metrics) : DiscordApplicationGuildModuleBase
{
    [SlashCommand(Metas.Stream.Channel)]
    [Description("Set the stream announce channel")]
    public async Task<DiscordInteractionResponseCommandResult> Set(IChannel channel)
    {
        using var _ = metrics.All<StreamCommands>();
        if (channel is not TransientInteractionChannel { Type: ChannelType.Text } textChannel)
            return Response(Localization.ChannelMustBeTextChannel);
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IStreamService>();
        var response = await service.SetChannel(Context.GuildId, textChannel.ToTextChannel());
        return Response(response);
    }

    [SlashCommand(Metas.Stream.Publish)]
    [Description("Toggle publishing when a configured user starts streaming")]
    public async Task<DiscordInteractionResponseCommandResult> TogglePublish()
    {
        using var _ = metrics.All<StreamCommands>();
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IStreamService>();
        var response = await service.TogglePublish(Context.GuildId);
        return Response(response);
    }

    [SlashCommand(Metas.Stream.Add)]
    [Description("Add a user who should trigger stream publish")]
    public async Task<DiscordInteractionResponseCommandResult> Add(IMember member, string twitch)
    {
        using var _ = metrics.All<StreamCommands>();
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IStreamService>();
        var response = await service.AddUser(Context.GuildId, member.Id, twitch);
        return Response(response);
    }

    [SlashCommand(Metas.Stream.Remove)]
    [Description("Remove a user from stream publish")]
    public async Task<DiscordInteractionResponseCommandResult> Remove(IMember member)
    {
        using var _ = metrics.All<StreamCommands>();
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IStreamService>();
        var removed = await service.RemoveUser(Context.GuildId, member.Id);
        return Response(removed
            ? Localization.StreamUserRemoved
            : Localization.StreamUserNotFound);
    }

    [SlashCommand(Metas.Stream.List)]
    [Description("List users configured for stream publish")]
    public async Task<IResult> List()
    {
        using var _ = metrics.All<StreamCommands>();
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IStreamService>();
        var response = await service.ListUsers(Context.GuildId);
        if (response.Value is not List<StreamUser> users) return Response(Localization.StreamNoUsersFound);

        var pages = new List<Page>();
        for (var i = 0; i < users.Count; i += 5)
        {
            var content = string.Empty;
            var end = Math.Min(i + 5, users.Count);
            for (var j = i; j < end; j++)
            {
                var x = users[j];
                content += $"<@{x.DiscordUserId}> — twitch.tv/{x.TwitchLogin}\n";
            }
            pages.Add(new Page().WithContent(content));
        }
        return Pages(pages);
    }

    [AutoComplete(Metas.Stream.Channel)]
    public void StreamChannelAutoComplete(AutoComplete<ITextChannel> channel)
    {
        if (!channel.IsFocused) return;
        var guild = Bot.GetGuild(Context.GuildId);
        if (guild is null) throw new ArgumentException("Couldn't get guild in auto-complete");

        var channels = guild.GetChannels();
        foreach (var x in channels)
            if (x.Value is CachedTextChannel textChannel)
                channel.Choices.Add($"{textChannel.Name}", textChannel);
    }
}
