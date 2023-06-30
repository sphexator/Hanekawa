using Hanekawa.Application.Contracts.Discord;
using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Discord;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Color = System.Drawing.Color;

namespace Hanekawa.Application.Handlers.Logs;

public class UserUnbannedHandler : IRequestHandler<UserUnbanned, bool>
{
    private readonly IBot _bot;
    private readonly IDbContext _db;

    public UserUnbannedHandler(IBot bot, IDbContext db)
    {
        _bot = bot;
        _db = db;
    }
    
    public async Task<bool> Handle(UserUnbanned request, CancellationToken cancellationToken)
    {
        var cfg = await _db.GuildConfigs.Include(x => x.LogConfig)
            .FirstOrDefaultAsync(x => x.GuildId == request.Member.GuildId, cancellationToken: cancellationToken);
        if (cfg is { LogConfig.ModLogChannelId: null }) return false;
        var channel = _bot.GetChannel(request.Member.GuildId, cfg.LogConfig.ModLogChannelId.Value);
        if (channel is null)
        {
            cfg.LogConfig.ModLogChannelId = null;
            await _db.SaveChangesAsync();
            return false;
        }
        
        await _bot.SendMessageAsync(channel.Value, new Embed
        {
            Title = $"User Banned | Case ID: {request.Member.UserId} | ${request.Member.GuildId}",
            Color = Color.LimeGreen.ToArgb(),
            Fields = new List<EmbedField>
            {
                new("User", $"<@{request.Member.UserId}>", false),
                new("Moderator", "N/A", false),
                new("Reason", "No reason provided", false),
            }
        });
        return true;
    }
}