namespace Hanekawa.Entities;

/// <summary>
/// Represents a module in the system.
/// </summary>
public class Module
{
	public required ulong GuildId { get; init; }
	public required string Name { get; init; } = string.Empty;
	public required bool Enabled { get; init; } = false;
}