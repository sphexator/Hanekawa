using Hanekawa.Entities.Discord;
using MediatR;

namespace Hanekawa.Application.Contracts.Discord;

public record UserBanned(DiscordMember Member) : IRequest<bool>;