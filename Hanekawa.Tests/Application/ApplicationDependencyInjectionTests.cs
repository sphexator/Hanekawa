using Hanekawa.Application;
using Hanekawa.Application.Contracts.Discord.Services;
using Hanekawa.Application.Handlers.Services;
using Hanekawa.Application.Handlers.Services.Activity;
using Hanekawa.Application.Handlers.Services.Levels;
using Hanekawa.Application.Interfaces.Services;
using Hanekawa.Application.Services;
using Hanekawa.Decorator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Tests.Application;

public class ApplicationDependencyInjectionTests
{
    [Fact]
    public void AddApplicationLayer_RegistersSharedMessagePipelineHandlers()
    {
        var botProjectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "..", "..", "..", "..", "Hanekawa.Bot"));
        Directory.SetCurrentDirectory(botProjectDir);

        var services = new ServiceCollection();
        services.AddApplicationLayer(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build());

        Assert.Contains(services, d =>
            d.ServiceType == typeof(IActivityService) && d.ImplementationType == typeof(ActivityService));

        Assert.Contains(services, d =>
            d.ServiceType == typeof(INotificationHandler<MessageReceived>) &&
            d.ImplementationType == typeof(MessageRateLimitHandler));

        var passedHandlers = services.Where(d =>
            d.ServiceType == typeof(INotificationHandler<MessageRateLimitPassed>)).ToList();
        Assert.Equal(2, passedHandlers.Count);
        Assert.Contains(passedHandlers, d => d.ImplementationType == typeof(MessageReceivedExperienceHandler));
        Assert.Contains(passedHandlers, d => d.ImplementationType == typeof(WeeklyActivityHandler));
    }
}
