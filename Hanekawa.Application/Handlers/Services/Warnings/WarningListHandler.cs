using Hanekawa.Application.Interfaces;
using Hanekawa.Entities;
using Hanekawa.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Application.Handlers.Services.Warnings;

public record WarningList(ulong GuildId, ulong? UserId) : IRequest<Response<Pagination<Message>>>;

public class WarningListHandler(IDbContext db ) : IRequestHandler<WarningList, Response<Pagination<Message>>>
{
    public async Task<Response<Pagination<Message>>> Handle(WarningList request, CancellationToken cancellationToken)
    {
        var result = await db.Warnings.Where(x => 
                x.GuildId == request.GuildId && 
                x.UserId == request.UserId)
            .ToArrayAsync(cancellationToken: cancellationToken);
        if (result.Length == 0)
        {
            return new(new([new("No warnings found")]));
        }

        var pages = result.BuildPage().Paginate<Message>();
        return new(new(pages));
    }
}