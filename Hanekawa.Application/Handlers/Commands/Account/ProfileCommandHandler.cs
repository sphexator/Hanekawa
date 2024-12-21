using Hanekawa.Application.Interfaces;
using Hanekawa.Entities.Discord;
using MediatR;

namespace Hanekawa.Application.Handlers.Commands.Account;

public record ProfileCommand(DiscordMember Member) : IRequest<ProfileCommandResult>;
public record ProfileCommandResult(Stream ImageStream);

public class ProfileCommandHandler(IImageService imageService, IDbContext db) : IRequestHandler<ProfileCommand, ProfileCommandResult>
{
    public async Task<ProfileCommandResult> Handle(ProfileCommand request,
        CancellationToken cancellationToken)
    {
        var user = await db.Users.FindAsync([request.Member.Guild, request.Member.Id], cancellationToken: cancellationToken);
        return new ProfileCommandResult(await imageService.DrawProfileAsync(request.Member, user, cancellationToken));
    }
}