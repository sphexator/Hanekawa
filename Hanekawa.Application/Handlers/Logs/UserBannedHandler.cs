using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Discord;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Color = System.Drawing.Color;

namespace Hanekawa.Application.Handlers.Logs;

public class UserBannedHandler : IRequestHandler<Contracts.Discord.UserBanned, bool>
{
    private readonly IBot _bot;
    private readonly IDbContext _db;

    public UserBannedHandler(IBot bot, IDbContext db)
    {
        _bot = bot;
        _db = db;
    }

    public async Task<bool> Handle(Contracts.Discord.UserBanned request, CancellationToken cancellationToken)
    {
        var cfg = await _db.GuildConfigs.Include(x => x.LogConfig)
            .FirstOrDefaultAsync(x => x.GuildId == request.GuildId, cancellationToken: cancellationToken);
        if (!cfg.LogConfig.ModLogChannelId.HasValue) return false;
        var channel = _bot.GetChannel(request.GuildId, cfg.LogConfig.ModLogChannelId.Value);
        if (channel is null)
        {
            cfg.LogConfig.ModLogChannelId = null;
            await _db.SaveChangesAsync();
            return false;
        }
        
        await _bot.SendMessageAsync(channel.Value, new Embed
        {
            Title = $"User Banned | Case ID: {request.UserId} | ${request.GuildId}",
            Color = Color.Red.ToArgb(),
            Fields = new List<EmbedField>
            {
                new("User", $"request.UserId", false),
                new("Moderator", "N/A", false),
                new("Reason", "No reason provided", false),
            }
        });
        return true;
    }
}