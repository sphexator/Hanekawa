using Hanekawa.Application.Services;
using Hanekawa.Decorator;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Tests.Mediatr;

public class DispatcherLifetimeTests
{
    private sealed class TestNotification;

    private sealed class TestRequest;

    private sealed class CallTracker
    {
        public int Count { get; set; }
        public bool SawScopedDependency { get; set; }
    }

    private sealed class ScopedDependency;

    private sealed class TestNotificationHandler(ScopedDependency scoped, CallTracker tracker)
        : INotificationHandler<TestNotification>
    {
        public Task HandleAsync(TestNotification notification, CancellationToken cancellationToken = default)
        {
            tracker.SawScopedDependency = scoped is not null;
            tracker.Count++;
            return Task.CompletedTask;
        }
    }

    private sealed class TestRequestHandler(ScopedDependency scoped, CallTracker tracker)
        : IRequestHandler<TestRequest, string>
    {
        public Task<string> HandleAsync(TestRequest request, CancellationToken cancellationToken = default)
        {
            tracker.SawScopedDependency = scoped is not null;
            tracker.Count++;
            return Task.FromResult("ok");
        }
    }

    private static ServiceProvider BuildRootProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IEventPublisher, EventPublisher>();
        services.AddSingleton<IRequestDispatcher, RequestDispatcher>();
        services.AddSingleton<CallTracker>();
        services.AddScoped<ScopedDependency>();
        services.AddScoped<INotificationHandler<TestNotification>, TestNotificationHandler>();
        services.AddScoped<IRequestHandler<TestRequest, string>, TestRequestHandler>();
        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });
    }

    [Fact]
    public async Task EventPublisher_ResolvedFromRoot_WithValidateScopes_InvokesScopedHandler()
    {
        await using var provider = BuildRootProvider();
        var publisher = provider.GetRequiredService<IEventPublisher>();

        await publisher.PublishAsync(new TestNotification());

        var tracker = provider.GetRequiredService<CallTracker>();
        Assert.Equal(1, tracker.Count);
        Assert.True(tracker.SawScopedDependency);
    }

    [Fact]
    public async Task RequestDispatcher_ResolvedFromRoot_WithValidateScopes_InvokesScopedHandler()
    {
        await using var provider = BuildRootProvider();
        var dispatcher = provider.GetRequiredService<IRequestDispatcher>();

        var result = await dispatcher.SendAsync<TestRequest, string>(new TestRequest());

        Assert.Equal("ok", result);
        var tracker = provider.GetRequiredService<CallTracker>();
        Assert.Equal(1, tracker.Count);
        Assert.True(tracker.SawScopedDependency);
    }
}
