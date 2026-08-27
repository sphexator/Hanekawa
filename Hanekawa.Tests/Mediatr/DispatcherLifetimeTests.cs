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
        services.AddScoped<IRequestHandler<TestRequest>, TestVoidHandler>();
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

    [Fact]
    public async Task RequestDispatcher_VoidSend_ResolvedFromRoot_InvokesScopedHandler()
    {
        await using var provider = BuildRootProvider();
        var dispatcher = provider.GetRequiredService<IRequestDispatcher>();

        await dispatcher.SendAsync(new TestRequest());

        var tracker = provider.GetRequiredService<CallTracker>();
        Assert.Equal(1, tracker.Count);
        Assert.True(tracker.SawScopedDependency);
    }

    [Fact]
    public async Task EventPublisher_InvokesAllRegisteredHandlers()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IEventPublisher, EventPublisher>();
        services.AddSingleton<CallTracker>();
        services.AddScoped<ScopedDependency>();
        services.AddScoped<INotificationHandler<TestNotification>, TestNotificationHandler>();
        services.AddScoped<INotificationHandler<TestNotification>, SecondNotificationHandler>();
        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });

        await provider.GetRequiredService<IEventPublisher>().PublishAsync(new TestNotification());

        Assert.Equal(2, provider.GetRequiredService<CallTracker>().Count);
    }

    [Fact]
    public async Task EventPublisher_WithNoHandlers_DoesNotThrow()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IEventPublisher, EventPublisher>();
        await using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });

        await provider.GetRequiredService<IEventPublisher>().PublishAsync(new TestNotification());
    }

    private sealed class TestVoidHandler(ScopedDependency scoped, CallTracker tracker) : IRequestHandler<TestRequest>
    {
        public Task HandleAsync(TestRequest request, CancellationToken cancellationToken = default)
        {
            tracker.SawScopedDependency = scoped is not null;
            tracker.Count++;
            return Task.CompletedTask;
        }
    }

    private sealed class SecondNotificationHandler(CallTracker tracker) : INotificationHandler<TestNotification>
    {
        public Task HandleAsync(TestNotification notification, CancellationToken cancellationToken = default)
        {
            tracker.Count++;
            return Task.CompletedTask;
        }
    }
}
