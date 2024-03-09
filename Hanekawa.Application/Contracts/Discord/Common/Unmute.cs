using Hanekawa.Application.Interfaces;
using Hanekawa.Entities;

namespace Hanekawa.Application.Contracts.Discord.Common;

public class Unmute : ISqs
{
	public ulong GuildId { get; init; }
	public ulong UserId { get; init; }
	public ulong ModeratorId { get; init; }
	public string Reason { get; init; } = string.Empty;

	public ProviderSource Source { get; init; } = ProviderSource.Discord;
}