using System.Runtime.InteropServices;
using Hanekawa.Application.Interfaces;
using Hanekawa.Entities;
using Hanekawa.Entities.Configs;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Application.Handlers.Commands.Boost;

public interface IBoostCommandService
{
    ValueTask<Response<GuildConfig>?> ListAsync(ulong guildId);
}

internal class BoostCommands : IBoostCommandService
{
    private readonly IDbContext _context;

    public BoostCommands(IDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Response<GuildConfig>?> ListAsync(ulong guildId)
    {
        var config = await _context.GuildConfigs
            .Include(e => e.BoostConfig)
            .FirstOrDefaultAsync(x => x.GuildId == guildId);
        return config is null or { BoostConfig: null}
            ? null
            : new Response<GuildConfig>(config);
    }
}