using Hanekawa.Application.Interfaces;
using Hanekawa.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Application.Extensions;

internal static class ProviderSourceExtensions
{
	internal static IBot GetClient(this ProviderSource source, IServiceProvider services)
		=> source switch
		{
				ProviderSource.Discord => services.GetRequiredKeyedService<IBot>(nameof(ProviderSource.Discord)),
				_                      => throw new NotImplementedException()
		};
}