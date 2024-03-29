﻿using Disqord;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Application;
using Disqord.Bot.Commands.Interaction;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Gateway;
using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Interfaces.Commands;
using Hanekawa.Bot.Commands.Metas;
using Hanekawa.Bot.Mapper;
using Hanekawa.Entities.Configs;
using Hanekawa.Localize;
using Qmmands;
using IResult = Qmmands.IResult;

namespace Hanekawa.Bot.Commands.Slash.Setting;

[SlashGroup(SlashGroupName.Greet)]
[RequireAuthorPermissions(Permissions.ManageChannels)]
public class GreetCommands(IMetrics metrics) : DiscordApplicationGuildModuleBase
{
    [SlashCommand(Greet.Channel)]
    [Description("Set the greet channel")]
    public async Task<DiscordInteractionResponseCommandResult> Set(IChannel channel)
    {
        using var _ = metrics.MeasureDuration<GreetCommands>();
        metrics.IncrementCounter<GreetCommands>();
        if (channel is not TransientInteractionChannel { Type: ChannelType.Text } textChannel) 
            return Response(Localization.ChannelMustBeTextChannel);
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IGreetService>();
        var response = await service.SetChannel(Context.GuildId, textChannel.ToTextChannel());
        return Response(response);
    }
    
    [SlashCommand(Greet.Message)]
    [Description("Set the greet message")]
    public async Task<DiscordInteractionResponseCommandResult> Set(string message)
    {
        using var _ = metrics.MeasureDuration<GreetCommands>();
        metrics.IncrementCounter<GreetCommands>();
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IGreetService>();
        var response = await service.SetMessage(Context.GuildId, message);
        return Response(response);
    }
    
    [SlashCommand(Greet.ImageUrl)]
    [Description("Set the greet image url")]
    public async Task<DiscordInteractionResponseCommandResult> SetImage(string url)
    {
        using var _ = metrics.MeasureDuration<GreetCommands>();
        metrics.IncrementCounter<GreetCommands>();
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IGreetService>();
        var response = await service.SetImage(Context.GuildId, url, Context.AuthorId);
        return Response(response);
    }
    
    [SlashCommand(Greet.ImageList)]
    [Description("List the greet images")]
    public async Task<IResult> ListImages()
    {
        using var _ = metrics.MeasureDuration<GreetCommands>();
        metrics.IncrementCounter<GreetCommands>();
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IGreetService>();
        var response = await service.ListImages(Context.GuildId);
        if (response.Value is not List<GreetImage> images) return Response(Localization.NoImagesFound);
        
        var pages = new List<Page>();
        for (var i = 0; i < images.Count / 5; i++)
        {
            var page = new Page();
            for (var j = 0; j < 5; j++)
            {
                var x = images[j + (5 * i)];
                page.WithContent($"ID: {x.Id}\nURL: {x.ImageUrl}\n");
            }
            pages.Add(page);
        }
        return Pages(pages);
    }
    
    [SlashCommand(Greet.ImageRemove)]
    [Description("Remove the greet image")]
    public async Task<DiscordInteractionResponseCommandResult> RemoveImage(int id)
    {
        using var _ = metrics.MeasureDuration<GreetCommands>();
        metrics.IncrementCounter<GreetCommands>();
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IGreetService>();
        var response = await service.RemoveImage(Context.GuildId, id);
        return Response(response 
            ? Localization.RemovedImage 
            : Localization.NoImageWithIdFound);
    }
    
    [SlashCommand(Greet.ImageToggle)]
    [Description("Toggle the greet image")]
    public async Task<DiscordInteractionResponseCommandResult> ToggleImage()
    {
        using var _ = metrics.MeasureDuration<GreetCommands>();
        metrics.IncrementCounter<GreetCommands>();
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IGreetService>();
        var response = await service.ToggleImage(Context.GuildId);
        return Response(response);
    }

    [AutoComplete(Greet.Channel)]
    public void GreetAutoComplete(AutoComplete<ITextChannel> channel)
    {
        if (!channel.IsFocused) return;
        var guild = Bot.GetGuild(Context.GuildId);
        if (guild is null) throw new ArgumentException("Couldn't get guild in auto-complete");
            
        var channels = guild.GetChannels();
        foreach (var x in channels) 
            if (x.Value is CachedTextChannel textChannel) 
                channel.Choices.Add($"{textChannel.Name}", textChannel);
    }
    
    [AutoComplete(Greet.ImageRemove)]
    public async Task RemoveImage(AutoComplete<int> id)
    {
        if(!id.IsFocused) return;
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IGreetService>();
        var response = await service.ListImages(Context.GuildId);
        if (response.Value is not List<GreetImage> images) return;
        foreach (var x in images) id.Choices.Add($"{x.Id} - {x.ImageUrl}", x.Id);
    }
}