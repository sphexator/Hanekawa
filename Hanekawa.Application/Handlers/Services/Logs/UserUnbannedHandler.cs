using Hanekawa.Application.Contracts.Discord.Services;
using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Interfaces.Services;
using Hanekawa.Decorator;
using Hanekawa.Entities;
using Hanekawa.Entities.Discord;
using Microsoft.EntityFrameworkCore;
using Color = System.Drawing.Color;

namespace Hanekawa.Application.Handlers.Services.Logs;

public class UserUnbannedHandler : INotificationHandler<UserUnbanned>
{
    private readonly IBot _bot;
    private readonly IDbContext _db;
    private readonly IModuleService _moduleService;

    public UserUnbannedHandler(IBot bot, IDbContext db, IModuleService moduleService)
    {
        _bot = bot;
        _db = db;
        _moduleService = moduleService;
    }

    public async Task HandleAsync(UserUnbanned notification, CancellationToken cancellationToken)
    {
        if (!await _moduleService.IsEnabledAsync(notification.Member.Guild.GuildId, ModuleName.Logging, cancellationToken))
            return;

        var cfg = await _db.GuildConfigs.Include(x => x.LogConfig)
            .FirstOrDefaultAsync(x => x.GuildId == notification.Member.Guild.GuildId, cancellationToken: cancellationToken);
        if (cfg is { LogConfig.ModLogChannelId: null }) return;
        var channel = _bot.GetChannel(notification.Member.Guild.GuildId, cfg.LogConfig.ModLogChannelId.Value);
        if (channel is null)
        {
            cfg.LogConfig.ModLogChannelId = null;
            await _db.SaveChangesAsync(cancellationToken);
            return;
        }

        await _bot.SendMessageAsync(channel.Value, new Embed
        {
            Title = $"User Banned | Case ID: {notification.Member.Id} | ${notification.Member.Guild.GuildId}",
            Color = Color.LimeGreen.ToArgb(),
            Fields =
            [
                new EmbedField("User", $"<@{notification.Member.Id}>", false),
                new EmbedField("Moderator", "N/A", false),
                new EmbedField("Reason", "No reason provided", false)
            ]
        });
        return;
    }
}