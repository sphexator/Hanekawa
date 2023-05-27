using Hanekawa.Application.Interfaces;
using MediatR;

namespace Hanekawa.Application.Handlers.Warnings;

public record WarningReceived(ulong GuildId, ulong UserId, string Warning, ulong ModeratorId) : ISqs;

public class WarningReceivedHandler : IRequestHandler<WarningReceived>
{
    private readonly IDbContext _db;
    public WarningReceivedHandler(IDbContext db) => _db = db;

    public async Task Handle(WarningReceived request, CancellationToken cancellationToken)
    {
        await _db.Warnings.AddAsync(new()
        {
            Id = Guid.NewGuid(),
            GuildId = request.GuildId,
            UserId = request.UserId,
            ModeratorId = request.ModeratorId,
            Reason = request.Warning,
            Valid = true,
            CreatedAt = DateTimeOffset.UtcNow
        }, cancellationToken);
        await _db.SaveChangesAsync();
    }
}