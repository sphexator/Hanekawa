using Hanekawa.Entities;
using MediatR;

namespace Hanekawa.Application.Handlers.Services.Warnings;

public record WarningList(ulong GuildId, ulong? UserId) : IRequest<Response<Message>>;

public class WarningListHandler : IRequestHandler<WarningList, Response<Message>>
{
    public Task<Response<Message>> Handle(WarningList request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}