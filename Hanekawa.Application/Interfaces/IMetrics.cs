namespace Hanekawa.Application.Interfaces;

public interface IMetrics
{
	public TrackedDuration All<T>(ulong? guildId = null);

	public TrackedDuration All(string name, ulong? guildId = null);
	
	public void IncrementCounter<T>(ulong? guildId = null);
	public void IncrementCounter(string name, ulong? guildId = null);
	public TrackedDuration MeasureDuration<T>(ulong? guildId = null);
	public TrackedDuration MeasureDuration(string name, ulong? guildId = null);
}