using Hanekawa.Entities.Discord;
using MediatR;

namespace Hanekawa.Application.Contracts.Discord.Services;

public record UserUnbanned(DiscordMember Member) : IRequest<bool>;