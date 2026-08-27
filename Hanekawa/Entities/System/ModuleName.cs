namespace Hanekawa.Entities;

/// <summary>
/// Canonical names of toggleable modules.
/// </summary>
public static class ModuleName
{
	public const string Administration = "Administration";
	public const string Account = "Account";
	public const string Level = "Level";
	public const string Club = "Club";
	public const string Boost = "Boost";
	public const string Greet = "Greet";
	public const string Logging = "Logging";

	public static readonly string[] All =
	[
		Administration,
		Account,
		Level,
		Club,
		Boost,
		Greet,
		Logging
	];
}
