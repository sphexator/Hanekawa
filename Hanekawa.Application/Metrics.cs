using System.Diagnostics.Metrics;
using Hanekawa.Application.Interfaces;

namespace Hanekawa.Application;

public class Metrics(IMeterFactory meterFactory) : IMetrics
{
	private readonly Dictionary<string, MetricCollection> _metrics = new();

	/// <inheritdoc />
	public TrackedDuration All<T>(ulong? guildId = null) => All(nameof(T), guildId);

	/// <inheritdoc />
	public TrackedDuration All(string name, ulong? guildId = null)
	{
		if(!_metrics.TryGetValue(name, out var metric)) metric = CreateMetric(name, guildId);
		metric.GlobalCounter.Add(1);
		if(guildId is not null) metric.GuildCounter[guildId.Value].Add(1);
		return new (TimeProvider.System, metric.Histogram);
	}

	/// <inheritdoc />
	public void IncrementCounter<T>(ulong? guildId = null) => IncrementCounter(nameof(T));

	/// <inheritdoc />
	public void IncrementCounter(string name, ulong? guildId = null)
	{ 
		if(_metrics.TryGetValue(name, out var metric))
		{
			metric.GlobalCounter.Add(1);
			return;
		}
		metric = CreateMetric(name);
		metric.GlobalCounter.Add(1);
	}

	/// <inheritdoc />
	public TrackedDuration MeasureDuration<T>(ulong? guildId = null) => MeasureDuration(nameof(T));

	/// <inheritdoc />
	public TrackedDuration MeasureDuration(string name, ulong? guildId = null)
	{
		if (_metrics.TryGetValue(name, out var metric))
			return new(TimeProvider.System, metric.Histogram);
		var collection = CreateMetric(name);
		return new(TimeProvider.System, collection.Histogram);
	}

	private MetricCollection CreateMetric(string name, ulong? guildId = null)
	{
		var meter = meterFactory.Create($"hanekawa.{name}");
		var counter = meter.CreateCounter<long>($"hanekawa.{name}.request.counter");
		var guildCounter = new Dictionary<ulong, Counter<long>>();
		if (guildId != null)
		{
			guildCounter.Add(guildId.Value,
					meter.CreateCounter<long>($"hanekawa.{name}-{guildId.Value}.request.counter"));
		}
		var histogram = meter.CreateHistogram<double>($"hanekawa.{name}.request.duration");
		var collection = new MetricCollection(counter, guildCounter, histogram);
		_metrics.TryAdd(name, collection);
		return collection;
	}
}

public class TrackedDuration : IDisposable
{
	private readonly long _requestStartTime;
	private readonly Histogram<double> _histogram;
	private readonly TimeProvider _timeProvider;
	
	public TrackedDuration(TimeProvider timeProvider, Histogram<double> histogram)
	{
		_requestStartTime = timeProvider.GetTimestamp();
		_timeProvider = timeProvider;
		_histogram = histogram;
	}

	public void Dispose()
	{
		var elapsed = _timeProvider.GetElapsedTime(_requestStartTime);
		_histogram.Record(elapsed.TotalMilliseconds);
	}
}

internal class MetricCollection
{
	public MetricCollection(Counter<long> globalCounter, Dictionary<ulong, Counter<long>> guildCounter, Histogram<double> histogram)
	{
		GlobalCounter = globalCounter;
		GuildCounter = guildCounter;
		Histogram = histogram;
	}

	public Counter<long> GlobalCounter { get; }
	public Dictionary<ulong, Counter<long>> GuildCounter { get; }
	public Histogram<double> Histogram { get; }
}