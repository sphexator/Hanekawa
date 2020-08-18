using System.Threading.Tasks;
using Disqord;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Database.Tables.Config.Guild;
using Hanekawa.Database.Tables.Music;

namespace Hanekawa.Database.Extensions
{
    public static class ConfigExtension
    {
        public static async Task<GuildConfig> GetOrCreateGuildConfigAsync(this DbService context, CachedGuild guild) =>
            await GetOrCreateGuildConfigAsync(context, guild.Id.RawValue).ConfigureAwait(false);

        public static async Task<GuildConfig> GetOrCreateGuildConfigAsync(this DbService context, ulong guild)
        {
            var response = await context.GuildConfigs.FindAsync(guild).ConfigureAwait(false);
            if (response != null) return response;

            var data = new GuildConfig().DefaultGuildConfig(guild);
            try
            {
                await context.GuildConfigs.AddAsync(data).ConfigureAwait(false);
                context.GuildConfigs.Update(data);
                await context.SaveChangesAsync().ConfigureAwait(false);
                return await context.GuildConfigs.FindAsync(guild).ConfigureAwait(false);
            }
            catch
            {
                return data;
            }
        }

        public static GuildConfig GetOrCreateGuildConfig(this DbService context, CachedGuild guild) =>
            GetOrCreateGuildConfig(context, guild.Id.RawValue);

        public static GuildConfig GetOrCreateGuildConfig(this DbService context, ulong guild)
        {
            var response = context.GuildConfigs.Find(guild);
            if (response != null) return response;

            var data = new GuildConfig().DefaultGuildConfig(guild);
            try
            {
                context.GuildConfigs.Add(data);
                context.GuildConfigs.Update(data);
                context.SaveChanges();
                return context.GuildConfigs.Find(guild);
            }
            catch
            {
                return data;
            }
        }

        public static async Task<AdminConfig> GetOrCreateAdminConfigAsync(this DbService context, IGuild guild) =>
            await GetOrCreateAdminConfigAsync(context, guild.Id.RawValue).ConfigureAwait(false);

        public static async Task<AdminConfig> GetOrCreateAdminConfigAsync(this DbService context, ulong guild)
        {
            var response = await context.AdminConfigs.FindAsync(guild).ConfigureAwait(false);
            if (response != null) return response;

            var data = new AdminConfig().DefaultAdminConfig(guild);
            try
            {
                await context.AdminConfigs.AddAsync(data).ConfigureAwait(false);
                context.AdminConfigs.Update(data);
                await context.SaveChangesAsync().ConfigureAwait(false);
                return await context.AdminConfigs.FindAsync(guild).ConfigureAwait(false);
            }
            catch
            {
                return data;
            }
        }

        public static async Task<BoardConfig> GetOrCreateBoardConfigAsync(this DbService context, IGuild guild) =>
            await GetOrCreateBoardConfigAsync(context, guild.Id.RawValue).ConfigureAwait(false);

        public static async Task<BoardConfig> GetOrCreateBoardConfigAsync(this DbService context, ulong guild)
        {
            var response = await context.BoardConfigs.FindAsync(guild).ConfigureAwait(false);
            if (response != null) return response;

            var data = new BoardConfig().DefaultBoardConfig(guild);
            try
            {
                await context.BoardConfigs.AddAsync(data).ConfigureAwait(false);
                context.BoardConfigs.Update(data);
                await context.SaveChangesAsync().ConfigureAwait(false);
                return await context.BoardConfigs.FindAsync(guild).ConfigureAwait(false);
            }
            catch
            {
                return data;
            }
        }

        public static async Task<ChannelConfig> GetOrCreateChannelConfigAsync(this DbService context, IGuild guild) =>
            await GetOrCreateChannelConfigAsync(context, guild.Id.RawValue).ConfigureAwait(false);

        public static async Task<ChannelConfig> GetOrCreateChannelConfigAsync(this DbService context, ulong guild)
        {
            var response = await context.ChannelConfigs.FindAsync(guild).ConfigureAwait(false);
            if (response != null) return response;

            var data = new ChannelConfig().DefaultChannelConfig(guild);
            try
            {
                await context.ChannelConfigs.AddAsync(data).ConfigureAwait(false);
                context.ChannelConfigs.Update(data);
                await context.SaveChangesAsync().ConfigureAwait(false);
                return await context.ChannelConfigs.FindAsync(guild).ConfigureAwait(false);
            }
            catch
            {
                return data;
            }
        }

        public static async Task<ClubConfig> GetOrCreateClubConfigAsync(this DbService context, IGuild guild) =>
            await GetOrCreateClubConfigAsync(context, guild.Id.RawValue).ConfigureAwait(false);

        public static async Task<ClubConfig> GetOrCreateClubConfigAsync(this DbService context, ulong guild)
        {
            var response = await context.ClubConfigs.FindAsync(guild).ConfigureAwait(false);
            if (response != null) return response;
            var data = new ClubConfig().DefaultClubConfig(guild);
            try
            {
                await context.ClubConfigs.AddAsync(data).ConfigureAwait(false);
                context.ClubConfigs.Update(data);
                await context.SaveChangesAsync().ConfigureAwait(false);
                return await context.ClubConfigs.FindAsync(guild).ConfigureAwait(false);
            }
            catch
            {
                return data;
            }
        }

        public static async Task<CurrencyConfig> GetOrCreateCurrencyConfigAsync(this DbService context, IGuild guild) =>
            await GetOrCreateCurrencyConfigAsync(context, guild.Id.RawValue).ConfigureAwait(false);

        public static async Task<CurrencyConfig> GetOrCreateCurrencyConfigAsync(this DbService context, ulong guild)
        {
            var response = await context.CurrencyConfigs.FindAsync(guild).ConfigureAwait(false);
            if (response != null) return response;
            var data = new CurrencyConfig().DefaultCurrencyConfig(guild);
            try
            {
                await context.CurrencyConfigs.AddAsync(data).ConfigureAwait(false);
                context.CurrencyConfigs.Update(data);
                await context.SaveChangesAsync().ConfigureAwait(false);
                return await context.CurrencyConfigs.FindAsync(guild).ConfigureAwait(false);
            }
            catch
            {
                return data;
            }
        }

        public static async Task<LevelConfig> GetOrCreateLevelConfigAsync(this DbService context, IGuild guild) =>
            await GetOrCreateLevelConfigAsync(context, guild.Id.RawValue).ConfigureAwait(false);

        public static async Task<LevelConfig> GetOrCreateLevelConfigAsync(this DbService context, ulong guild)
        {
            var response = await context.LevelConfigs.FindAsync(guild).ConfigureAwait(false);
            if (response != null) return response;
            var data = new LevelConfig().DefaultLevelConfig(guild);
            try
            {
                await context.LevelConfigs.AddAsync(data).ConfigureAwait(false);
                context.LevelConfigs.Update(data);
                await context.SaveChangesAsync().ConfigureAwait(false);
                return await context.LevelConfigs.FindAsync(guild).ConfigureAwait(false);
            }
            catch
            {
                return data;
            }
        }

        public static async Task<LoggingConfig> GetOrCreateLoggingConfigAsync(this DbService context, IGuild guild) =>
            await GetOrCreateLoggingConfigAsync(context, guild.Id.RawValue).ConfigureAwait(false);

        public static async Task<LoggingConfig> GetOrCreateLoggingConfigAsync(this DbService context, ulong guild)
        {
            var response = await context.LoggingConfigs.FindAsync(guild).ConfigureAwait(false);
            if (response != null) return response;

            var data = new LoggingConfig().DefaultLoggingConfig(guild);
            try
            {
                await context.LoggingConfigs.AddAsync(data).ConfigureAwait(false);
                context.LoggingConfigs.Update(data);
                await context.SaveChangesAsync().ConfigureAwait(false);
                return await context.LoggingConfigs.FindAsync(guild).ConfigureAwait(false);
            }
            catch
            {
                return data;
            }
        }

        public static async Task<SuggestionConfig> GetOrCreateSuggestionConfigAsync(this DbService context,
            IGuild guild) =>
            await GetOrCreateSuggestionConfigAsync(context, guild.Id.RawValue).ConfigureAwait(false);

        public static async Task<SuggestionConfig> GetOrCreateSuggestionConfigAsync(this DbService context, ulong guild)
        {
            var response = await context.SuggestionConfigs.FindAsync(guild).ConfigureAwait(false);
            if (response != null) return response;

            var data = new SuggestionConfig().DefaultSuggestionConfig(guild);
            try
            {
                await context.SuggestionConfigs.AddAsync(data).ConfigureAwait(false);
                context.SuggestionConfigs.Update(data);
                await context.SaveChangesAsync().ConfigureAwait(false);
                return await context.SuggestionConfigs.FindAsync(guild).ConfigureAwait(false);
            }
            catch
            {
                return data;
            }
        }

        public static async Task<WelcomeConfig> GetOrCreateWelcomeConfigAsync(this DbService context, IGuild guild) =>
            await GetOrCreateWelcomeConfigAsync(context, guild.Id.RawValue).ConfigureAwait(false);

        public static async Task<WelcomeConfig> GetOrCreateWelcomeConfigAsync(this DbService context, ulong guild)
        {
            var response = await context.WelcomeConfigs.FindAsync(guild).ConfigureAwait(false);
            if (response != null) return response;

            var data = new WelcomeConfig().DefaultWelcomeConfig(guild);
            try
            {
                await context.WelcomeConfigs.AddAsync(data).ConfigureAwait(false);
                context.WelcomeConfigs.Update(data);
                await context.SaveChangesAsync().ConfigureAwait(false);
                return await context.WelcomeConfigs.FindAsync(guild).ConfigureAwait(false);
            }
            catch
            {
                return data;
            }
        }

        public static async Task<DropConfig> GetOrCreateDropConfig(this DbService context, IGuild guild) =>
            await GetOrCreateDropConfig(context, guild.Id.RawValue).ConfigureAwait(false);

        public static async Task<DropConfig> GetOrCreateDropConfig(this DbService context, ulong guildId)
        {
            var response = await context.DropConfigs.FindAsync(guildId).ConfigureAwait(false);
            if (response != null) return response;
            var data = new DropConfig().DefaultDropConfig(guildId);
            try
            {
                await context.DropConfigs.AddAsync(data).ConfigureAwait(false);
                context.DropConfigs.Update(data);
                await context.SaveChangesAsync().ConfigureAwait(false);
                return await context.DropConfigs.FindAsync(guildId).ConfigureAwait(false);
            }
            catch
            {
                return data;
            }
        }

        public static async Task<MusicConfig> GetOrCreateMusicConfig(this DbService context, CachedGuild guild) =>
            await GetOrCreateMusicConfig(context, guild.Id.RawValue).ConfigureAwait(false);

        public static async Task<MusicConfig> GetOrCreateMusicConfig(this DbService context, ulong guildId)
        {
            var response = await context.MusicConfigs.FindAsync(guildId).ConfigureAwait(false);
            if (response != null) return response;
            var data = new MusicConfig { GuildId = guildId };
            try
            {
                await context.MusicConfigs.AddAsync(data).ConfigureAwait(false);
                context.MusicConfigs.Update(data);
                await context.SaveChangesAsync().ConfigureAwait(false);
                return await context.MusicConfigs.FindAsync(guildId).ConfigureAwait(false);
            }
            catch
            {
                return data;
            }
        }
    }
}