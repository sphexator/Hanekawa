using System.Diagnostics.Metrics;
using Hanekawa.Application;
using Microsoft.Extensions.Diagnostics.Metrics;

namespace Hanekawa.Tests.Metrics;

public class MetricsTests
{
    [Fact]
    public void All_RecordsCounterAndDuration_AndReusesTheSameInstruments()
    {
        var counters = new List<Measurement>();
        var durations = new List<Measurement>();
        using var listener = Listen(counters, durations);
        var metrics = new global::Hanekawa.Application.Metrics(new TestMeterFactory());

        using (metrics.All("ban"))
        {
        }

        using (metrics.All("ban"))
        {
        }

        Assert.Equal(2, counters.Where(x => x.Name == "hanekawa.ban.request.counter").Sum(x => (long)x.Value));
        Assert.Equal(2, durations.Count(x => x.Name == "hanekawa.ban.request.duration"));
        Assert.All(durations, sample => Assert.True(sample.Value >= 0));
    }

    [Fact]
    public void All_WithTheSameGuild_IncrementsTheGuildCounterOnEveryCall()
    {
        var counters = new List<Measurement>();
        var durations = new List<Measurement>();
        using var listener = Listen(counters, durations);
        var metrics = new global::Hanekawa.Application.Metrics(new TestMeterFactory());

        using (metrics.All("ban", 42))
        {
        }

        using (metrics.All("ban", 42))
        {
        }

        Assert.Equal(2, counters.Where(x => x.Name == "hanekawa.ban.request.counter").Sum(x => (long)x.Value));
        Assert.Equal(2, counters.Where(x => x.Name == "hanekawa.ban-42.request.counter").Sum(x => (long)x.Value));
        Assert.Equal(2, durations.Count(x => x.Name == "hanekawa.ban.request.duration"));
    }

    [Fact]
    public void MeasureDuration_RecordsDuration_WithoutIncrementingTheCounter()
    {
        var counters = new List<Measurement>();
        var durations = new List<Measurement>();
        using var listener = Listen(counters, durations);
        var metrics = new global::Hanekawa.Application.Metrics(new TestMeterFactory());

        using (metrics.MeasureDuration("IRequest"))
        {
        }

        using (metrics.MeasureDuration("IRequest"))
        {
        }

        Assert.Empty(counters);
        Assert.Equal(2, durations.Count(x => x.Name == "hanekawa.IRequest.request.duration"));
    }

    [Fact]
    public void IncrementCounter_ThenMeasureDuration_RecordsBothOnTheSharedInstrument()
    {
        var counters = new List<Measurement>();
        var durations = new List<Measurement>();
        using var listener = Listen(counters, durations);
        var metrics = new global::Hanekawa.Application.Metrics(new TestMeterFactory());

        metrics.IncrementCounter("IRequest");
        metrics.IncrementCounter("IRequest");
        using (metrics.MeasureDuration("IRequest"))
        {
        }

        Assert.Equal(2, counters.Where(x => x.Name == "hanekawa.IRequest.request.counter").Sum(x => (long)x.Value));
        Assert.Single(durations, x => x.Name == "hanekawa.IRequest.request.duration");
    }

    [Fact]
    public void GenericCommandOverloads_RecordMeasurementsOnRepeatCalls()
    {
        var counters = new List<Measurement>();
        var durations = new List<Measurement>();
        using var listener = Listen(counters, durations);
        var metrics = new global::Hanekawa.Application.Metrics(new TestMeterFactory());

        using (metrics.All<MetricsTests>())
        {
        }

        using (metrics.All<MetricsTests>())
        {
        }

        metrics.IncrementCounter<MetricsTests>();
        using (metrics.MeasureDuration<MetricsTests>())
        {
        }

        Assert.True(counters.Sum(x => (long)x.Value) >= 3);
        Assert.True(durations.Count >= 3);
        Assert.All(durations, sample => Assert.True(sample.Value >= 0));
    }

    private static MeterListener Listen(List<Measurement> counters, List<Measurement> durations)
    {
        var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, meterListener) =>
        {
            if (instrument.Meter.Name.StartsWith("hanekawa.", StringComparison.Ordinal))
                meterListener.EnableMeasurementEvents(instrument);
        };
        listener.SetMeasurementEventCallback<long>((instrument, measurement, _, _) =>
            counters.Add(new Measurement(instrument.Name, measurement)));
        listener.SetMeasurementEventCallback<double>((instrument, measurement, _, _) =>
            durations.Add(new Measurement(instrument.Name, measurement)));
        listener.Start();
        return listener;
    }

    private sealed record Measurement(string Name, double Value);

    private sealed class TestMeterFactory : IMeterFactory
    {
        public Meter Create(MeterOptions options) => new(options);

        public void Dispose()
        {
        }
    }
}
