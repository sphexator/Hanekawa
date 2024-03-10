using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Interfaces.Commands;
using Hanekawa.Entities.Configs;
using Hanekawa.Entities.Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;

namespace Hanekawa.Application.Handlers.Commands.Settings;

public class GreetService(IDbContext db, ILogger<GreetService> logger) : IGreetService
{
    public async Task<string> SetChannel(ulong guildId, TextChannel channel)
    {
        logger.LogInformation("Setting greet channel to {Channel} for guild {Guild}", 
            channel.Id, guildId);
        var config = await db.GuildConfigs.Include(e => e.GreetConfig)
            .FirstOrDefaultAsync(e => e.GuildId == guildId);
        if (config?.GreetConfig is null)
        {
            config ??= new() { GuildId = guildId };
            config.GreetConfig = new() { GuildId = guildId };
            await db.GuildConfigs.AddAsync(config);
        }

        config.GreetConfig.Channel = channel.Id;
        await db.SaveChangesAsync();
        return $"Set greet channel to {channel.Mention} !";
    }
    
    public async Task<string> SetMessage(ulong guildId, string message)
    {
        logger.LogInformation("Setting greet message to {Message} for guild {Guild}", 
            message, guildId);
        var config = await db.GuildConfigs.Include(e => e.GreetConfig)
            .FirstOrDefaultAsync(e => e.GuildId == guildId);
        if (config?.GreetConfig is null)
        {
            config ??= new() { GuildId = guildId };
            config.GreetConfig = new() { GuildId = guildId };
            await db.GuildConfigs.AddAsync(config);
        }

        config.GreetConfig.Message = message;
        await db.SaveChangesAsync();
        return "Updated greet message !";
    }

    public async Task<string> SetImage(ulong guildId, string url, ulong uploaderId)
    {
        logger.LogInformation("Setting greet image to {Url} for guild {Guild}", url, guildId);
        var config = await db.GuildConfigs.Include(e => e.GreetConfig)
            .FirstOrDefaultAsync(e => e.GuildId == guildId);
        if (config?.GreetConfig is null)
        {
            config ??= new() { GuildId = guildId };
            config.GreetConfig = new() { GuildId = guildId };
            await db.GuildConfigs.AddAsync(config);
        }

        config.GreetConfig.Images.Add(new ()
        {
            GuildId = guildId,
            ImageUrl = url,
            Uploader = uploaderId,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();
        return "Updated greet image !";
    }

    public async Task<OneOf<NotFound, List<GreetImage>>> ListImages(ulong guildId)
    {
        logger.LogInformation("Listing greet images for guild {Guild}", guildId);
        var config = await db.GuildConfigs
            .Include(x => x.GreetConfig)
            .ThenInclude(x => x.Images)
            .FirstOrDefaultAsync(x => x.GuildId == guildId);

        if (config?.GreetConfig is null || config.GreetConfig.Images.Count == 0) return new NotFound();
        
        return config.GreetConfig.Images;
    }

    public async Task<bool> RemoveImage(ulong guildId, int id)
    {
        logger.LogInformation("Removing greet image {Id} for guild {Guild}", id, guildId);
        var config = await db.GuildConfigs
            .Include(x => x.GreetConfig)
            .ThenInclude(x => x.Images)
            .FirstOrDefaultAsync(x => x.GuildId == guildId);
        if (config?.GreetConfig is null) return false;
        for (var i = 0; i < config.GreetConfig.Images.Count; i++)
        {
            var x = config.GreetConfig.Images[i];
            if (x.Id != id) continue;
            config.GreetConfig.Images.RemoveAt(i);
            await db.SaveChangesAsync();
            return true;
        }
        logger.LogWarning("Could not find greet image {Id} for guild {Guild}", id, guildId);
        return false;
    }
    
    public async Task<string> ToggleImage(ulong guildId)
    {
        var config = await db.GuildConfigs.Include(e => e.GreetConfig)
            .FirstOrDefaultAsync(e => e.GuildId == guildId);
        if (config?.GreetConfig == null)
        {
            config ??= new() { GuildId = guildId };
            config.GreetConfig = new() { GuildId = guildId };
            await db.GuildConfigs.AddAsync(config);
        }

        logger.LogInformation("Toggling greet image for guild {Guild} from {Old} to {New}", guildId,
            config.GreetConfig.ImageEnabled, !config.GreetConfig.ImageEnabled);
        config.GreetConfig.ImageEnabled = !config.GreetConfig.ImageEnabled;
        await db.SaveChangesAsync();
        return $"{(config.GreetConfig.ImageEnabled ? "Enabled" : "Disabled")} greet image !";
    }
}