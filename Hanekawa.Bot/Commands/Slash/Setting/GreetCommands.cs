using Disqord;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Application;
using Disqord.Bot.Commands.Interaction;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Gateway;
using Hanekawa.Application.Interfaces.Commands;
using Hanekawa.Bot.Mapper;
using Hanekawa.Entities.Configs;
using Qmmands;
using IResult = Qmmands.IResult;

namespace Hanekawa.Bot.Commands.Slash.Setting;

[SlashGroup("greet")]
[RequireAuthorPermissions(Permissions.ManageChannels)]
public class GreetCommands : DiscordApplicationGuildModuleBase
{
    [SlashCommand("channel")]
    [Description("Set the greet channel")]
    public async Task<DiscordInteractionResponseCommandResult> Set(IChannel channel)
    {
        if (channel is not TransientInteractionChannel { Type: ChannelType.Text } textChannel) return Response("Channel must be a text channel !");
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IGreetService>();
        var response = await service.SetChannel(Context.GuildId, textChannel.ToTextChannel());
        return Response(response);
    }
    
    [SlashCommand("message")]
    [Description("Set the greet message")]
    public async Task<DiscordInteractionResponseCommandResult> Set(string message)
    {
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IGreetService>();
        var response = await service.SetMessage(Context.GuildId, message);
        return Response(response);
    }
    
    [SlashCommand("imageurl")]
    [Description("Set the greet image url")]
    public async Task<DiscordInteractionResponseCommandResult> SetImage(string url)
    {
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IGreetService>();
        var response = await service.SetImage(Context.GuildId, url, Context.AuthorId);
        return Response(response);
    }
    
    [SlashCommand("imagelist")]
    [Description("List the greet images")]
    public async Task<IResult> ListImages()
    {
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IGreetService>();
        var response = await service.ListImages(Context.GuildId);
        if (response.Value is not List<GreetImage> images) return Response("No images found");
        
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
    
    [SlashCommand("removeImage")]
    [Description("Remove the greet image")]
    public async Task<DiscordInteractionResponseCommandResult> RemoveImage(int id)
    {
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IGreetService>();
        var response = await service.RemoveImage(Context.GuildId, id);
        return Response(response 
            ? "Removed image" 
            : "Image with that ID not found");
    }
    
    [SlashCommand("image")]
    [Description("Toggle the greet image")]
    public async Task<DiscordInteractionResponseCommandResult> ToggleImage()
    {
        await using var scope = Bot.Services.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IGreetService>();
        var response = await service.ToggleImage(Context.GuildId);
        return Response(response);
    }

    [AutoComplete("channel")]
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
    
    [AutoComplete("removeImage")]
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