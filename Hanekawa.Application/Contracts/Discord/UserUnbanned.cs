using Hanekawa.Entities.Discord;
using MediatR;

namespace Hanekawa.Application.Contracts.Discord;

public record UserUnbanned(DiscordMember Member) : IRequest<bool>;