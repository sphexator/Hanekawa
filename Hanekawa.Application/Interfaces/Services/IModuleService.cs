using Hanekawa.Entities;

namespace Hanekawa.Application.Interfaces.Services;

public interface IModuleService
{
    /// <summary>
    /// Checks whether a module is enabled for a guild. Modules without a stored row are enabled by default.
    /// </summary>
    ValueTask<bool> IsEnabledAsync(ulong guildId, string module, CancellationToken cancellationToken = default);
    /// <summary>
    /// Enables or disables a module for a guild, creating the row if it doesn't exist.
    /// </summary>
    Task SetEnabledAsync(ulong guildId, string module, bool enabled, CancellationToken cancellationToken = default);
    /// <summary>
    /// Lists all known modules with their state for a guild.
    /// </summary>
    Task<IReadOnlyList<Module>> GetModulesAsync(ulong guildId, CancellationToken cancellationToken = default);
}
