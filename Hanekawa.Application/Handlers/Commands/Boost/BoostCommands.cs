using System.Runtime.InteropServices;
using Hanekawa.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Application.Handlers.Commands.Boost;

public interface IBoostCommandService
{
    ValueTask<(string, string)[]> ListAsync(ulong guildId);
}

internal class BoostCommands : IBoostCommandService
{
    private readonly IDbContext _context;

    public BoostCommands(IDbContext context)
    {
        _context = context;
    }

    public async ValueTask<(string, string)[]> ListAsync(ulong guildId)
    {
        var config = await _context.GuildConfigs
            .Include(e => e.BoostConfig)
            .FirstOrDefaultAsync(x => x.GuildId == guildId);

        if (config is { BoostConfig: null})
        {
            return [];
        }

        List<(string, string)> values = [];
        foreach (var x in config.BoostConfig.GetType().GetProperties())
        {
            var value = x.GetValue(config.BoostConfig);
            if (value != null)
            {
                values.Add((x.Name, value.ToString()));
            }
        }

        return CollectionsMarshal.AsSpan(values).ToArray();
    }
}