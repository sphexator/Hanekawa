using Hanekawa.Entities.Discord;
using MediatR;

namespace Hanekawa.Application.Contracts.Discord.Services;

public record UserBanned(DiscordMember Member) : IRequest<bool>, IRequest;