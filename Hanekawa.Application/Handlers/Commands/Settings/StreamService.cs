using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Interfaces.Commands;
using Hanekawa.Entities.Configs;
using Hanekawa.Entities.Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;

namespace Hanekawa.Application.Handlers.Commands.Settings;

public class StreamService(IDbContext db, ILogger<StreamService> logger) : IStreamService
{
    public async Task<string> SetChannel(ulong guildId, TextChannel channel)
    {
        logger.LogInformation("Setting stream channel to {Channel} for guild {Guild}",
            channel.Id, guildId);
        var config = await GetOrCreateConfig(guildId);
        config.StreamConfig!.Channel = channel.Id;
        await db.SaveChangesAsync();
        return $"Set stream announce channel to {channel.Mention} !";
    }

    public async Task<string> TogglePublish(ulong guildId)
    {
        var config = await GetOrCreateConfig(guildId);
        logger.LogInformation("Toggling stream publish for guild {Guild} from {Old} to {New}", guildId,
            config.StreamConfig!.PublishOnStart, !config.StreamConfig.PublishOnStart);
        config.StreamConfig.PublishOnStart = !config.StreamConfig.PublishOnStart;
        await db.SaveChangesAsync();
        return config.StreamConfig.PublishOnStart
            ? "Enabled publishing when a configured user starts streaming !"
            : "Disabled publishing when a configured user starts streaming !";
    }

    public async Task<string> AddUser(ulong guildId, ulong discordUserId, string twitchLogin)
    {
        var login = NormalizeTwitchLogin(twitchLogin);
        if (login is null)
        {
            logger.LogWarning("Invalid Twitch login {Login} for guild {Guild}", twitchLogin, guildId);
            return "Twitch login is invalid.";
        }

        var config = await GetOrCreateConfig(guildId);
        var users = config.StreamConfig!.Users;
        if (users.Any(x => x.DiscordUserId == discordUserId))
        {
            return "That Discord user is already configured for streaming.";
        }

        if (users.Any(x => x.TwitchLogin == login))
        {
            return "That Twitch login is already configured for streaming.";
        }

        logger.LogInformation("Adding stream user {User} ({Twitch}) for guild {Guild}",
            discordUserId, login, guildId);
        users.Add(new StreamUser
        {
            GuildId = guildId,
            DiscordUserId = discordUserId,
            TwitchLogin = login
        });
        await db.SaveChangesAsync();
        return $"Added Twitch {login} for <@{discordUserId}> !";
    }

    public async Task<bool> RemoveUser(ulong guildId, ulong discordUserId)
    {
        logger.LogInformation("Removing stream user {User} for guild {Guild}", discordUserId, guildId);
        var config = await db.GuildConfigs
            .Include(x => x.StreamConfig)
            .ThenInclude(x => x.Users)
            .FirstOrDefaultAsync(x => x.GuildId == guildId);
        if (config?.StreamConfig is null) return false;
        for (var i = 0; i < config.StreamConfig.Users.Count; i++)
        {
            var x = config.StreamConfig.Users[i];
            if (x.DiscordUserId != discordUserId) continue;
            config.StreamConfig.Users.RemoveAt(i);
            await db.SaveChangesAsync();
            return true;
        }

        logger.LogWarning("Could not find stream user {User} for guild {Guild}", discordUserId, guildId);
        return false;
    }

    public async Task<OneOf<NotFound, List<StreamUser>>> ListUsers(ulong guildId)
    {
        logger.LogInformation("Listing stream users for guild {Guild}", guildId);
        var config = await db.GuildConfigs
            .Include(x => x.StreamConfig)
            .ThenInclude(x => x.Users)
            .FirstOrDefaultAsync(x => x.GuildId == guildId);

        if (config?.StreamConfig is null || config.StreamConfig.Users.Count == 0) return new NotFound();

        return config.StreamConfig.Users;
    }

    internal static string? NormalizeTwitchLogin(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;

        var login = input.Trim();
        if (login.StartsWith('@')) login = login[1..];
        if (login.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            login = login["https://".Length..];
        else if (login.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            login = login["http://".Length..];
        if (login.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
            login = login[4..];
        if (login.StartsWith("twitch.tv/", StringComparison.OrdinalIgnoreCase))
            login = login["twitch.tv/".Length..];

        var separator = login.IndexOfAny(['/', '?', '#']);
        if (separator >= 0) login = login[..separator];

        login = login.ToLowerInvariant();
        if (login.Length is 0 or > 25) return null;
        foreach (var c in login)
        {
            if (char.IsAsciiLetterOrDigit(c) || c == '_') continue;
            return null;
        }

        return login;
    }

    private async Task<GuildConfig> GetOrCreateConfig(ulong guildId, CancellationToken cancellationToken = default)
    {
        var config = await db.GuildConfigs
            .Include(e => e.StreamConfig)
            .ThenInclude(e => e.Users)
            .FirstOrDefaultAsync(e => e.GuildId == guildId, cancellationToken: cancellationToken);
        var addToDb = false;
        if (config is null)
        {
            addToDb = true;
            config = new GuildConfig { GuildId = guildId };
        }

        config.StreamConfig ??= new StreamConfig { GuildId = guildId };
        config.StreamConfig.Users ??= [];
        if (addToDb)
        {
            await db.GuildConfigs.AddAsync(config, cancellationToken);
        }

        return config;
    }
}
