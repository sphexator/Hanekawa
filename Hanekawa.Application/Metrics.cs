using System.Diagnostics.Metrics;

namespace Hanekawa.Application;

public class Metrics<T> where T : class
{
	private readonly Counter<long> _counter;
	private readonly Histogram<double> _histogram;
	
	public Metrics(IMeterFactory meterFactory)
	{
		var meter = meterFactory.Create(nameof(T));

		_counter = meter.CreateCounter<long>($"hanekawa.{nameof(T)}.request.counter");
		_histogram = meter.CreateHistogram<double>($"hanekawa.{nameof(T)}.request.duration");
	}
	
	public void IncrementCounter() => _counter.Add(1);
	public TrackedDuration MeasureDuration() => new(TimeProvider.System, _histogram);
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