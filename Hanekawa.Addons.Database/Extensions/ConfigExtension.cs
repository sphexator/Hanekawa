using System.Threading.Tasks;
using Discord;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Database.Tables.Config.Guild;

namespace Hanekawa.Database.Extensions
{
    public static class ConfigExtension
    {
        public static async Task<GuildConfig> GetOrCreateGuildConfigAsync(this DbService context, IGuild guild) =>
            await GetOrCreateGuildConfigAsync(context, guild.Id);

        public static async Task<GuildConfig> GetOrCreateGuildConfigAsync(this DbService context, ulong guild)
        {
            var response = await context.GuildConfigs.FindAsync(guild);
            if (response != null) return response;

            var data = new GuildConfig().DefaultGuildConfig(guild);
            try
            {
                await context.GuildConfigs.AddAsync(data);
                await context.SaveChangesAsync();
                return await context.GuildConfigs.FindAsync(guild);
            }
            catch
            {
                return data;
            }
        }

        public static GuildConfig GetOrCreateGuildConfig(this DbService context, IGuild guild) =>
            GetOrCreateGuildConfig(context, guild.Id);

        public static GuildConfig GetOrCreateGuildConfig(this DbService context, ulong guild)
        {
            var response = context.GuildConfigs.Find(guild);
            if (response != null) return response;

            var data = new GuildConfig().DefaultGuildConfig(guild);
            try
            {
                context.GuildConfigs.Add(data);
                context.SaveChanges();
                return context.GuildConfigs.Find(guild);
            }
            catch
            {
                return data;
            }
        }

        public static async Task<AdminConfig> GetOrCreateAdminConfigAsync(this DbService context, IGuild guild) =>
            await GetOrCreateAdminConfigAsync(context, guild.Id);

        public static async Task<AdminConfig> GetOrCreateAdminConfigAsync(this DbService context, ulong guild)
        {
            var response = await context.AdminConfigs.FindAsync(guild);
            if (response != null) return response;

            var data = new AdminConfig().DefaultAdminConfig(guild);
            try
            {
                await context.AdminConfigs.AddAsync(data);
                await context.SaveChangesAsync();
                return await context.AdminConfigs.FindAsync(guild);
            }
            catch
            {
                return data;
            }
        }

        public static async Task<BoardConfig> GetOrCreateBoardConfigAsync(this DbService context, IGuild guild) =>
            await GetOrCreateBoardConfigAsync(context, guild.Id);

        public static async Task<BoardConfig> GetOrCreateBoardConfigAsync(this DbService context, ulong guild)
        {
            var response = await context.BoardConfigs.FindAsync(guild);
            if (response != null) return response;

            var data = new BoardConfig().DefaultBoardConfig(guild);
            try
            {
                await context.BoardConfigs.AddAsync(data);
                await context.SaveChangesAsync();
                return await context.BoardConfigs.FindAsync(guild);
            }
            catch
            {
                return data;
            }
        }

        public static async Task<ChannelConfig> GetOrCreateChannelConfigAsync(this DbService context, IGuild guild) =>
            await GetOrCreateChannelConfigAsync(context, guild.Id);

        public static async Task<ChannelConfig> GetOrCreateChannelConfigAsync(this DbService context, ulong guild)
        {
            var response = await context.ChannelConfigs.FindAsync(guild);
            if (response != null) return response;

            var data = new ChannelConfig().DefaultChannelConfig(guild);
            try
            {
                await context.ChannelConfigs.AddAsync(data);
                await context.SaveChangesAsync();
                return await context.ChannelConfigs.FindAsync(guild);
            }
            catch
            {
                return data;
            }
        }

        public static async Task<ClubConfig> GetOrCreateClubConfigAsync(this DbService context, IGuild guild) =>
            await GetOrCreateClubConfigAsync(context, guild.Id);

        public static async Task<ClubConfig> GetOrCreateClubConfigAsync(this DbService context, ulong guild)
        {
            var response = await context.ClubConfigs.FindAsync(guild);
            if (response != null) return response;
            var data = new ClubConfig().DefaultClubConfig(guild);
            try
            {
                await context.ClubConfigs.AddAsync(data);
                await context.SaveChangesAsync();
                return await context.ClubConfigs.FindAsync(guild);
            }
            catch
            {
                return data;
            }
        }

        public static async Task<CurrencyConfig> GetOrCreateCurrencyConfigAsync(this DbService context, IGuild guild) =>
            await GetOrCreateCurrencyConfigAsync(context, guild.Id);

        public static async Task<CurrencyConfig> GetOrCreateCurrencyConfigAsync(this DbService context, ulong guild)
        {
            var response = await context.CurrencyConfigs.FindAsync(guild);
            if (response != null) return response;
            var data = new CurrencyConfig().DefaultCurrencyConfig(guild);
            try
            {
                await context.CurrencyConfigs.AddAsync(data);
                await context.SaveChangesAsync();
                return await context.CurrencyConfigs.FindAsync(guild);
            }
            catch
            {
                return data;
            }
        }

        public static async Task<LevelConfig> GetOrCreateLevelConfigAsync(this DbService context, IGuild guild) =>
            await GetOrCreateLevelConfigAsync(context, guild.Id);

        public static async Task<LevelConfig> GetOrCreateLevelConfigAsync(this DbService context, ulong guild)
        {
            var response = await context.LevelConfigs.FindAsync(guild);
            if (response != null) return response;
            var data = new LevelConfig().DefaultLevelConfig(guild);
            try
            {
                await context.LevelConfigs.AddAsync(data);
                await context.SaveChangesAsync();
                return await context.LevelConfigs.FindAsync(guild);
            }
            catch
            {
                return data;
            }
        }

        public static async Task<LoggingConfig> GetOrCreateLoggingConfigAsync(this DbService context, IGuild guild) =>
            await GetOrCreateLoggingConfigAsync(context, guild.Id);

        public static async Task<LoggingConfig> GetOrCreateLoggingConfigAsync(this DbService context, ulong guild)
        {
            var response = await context.LoggingConfigs.FindAsync(guild);
            if (response != null) return response;

            var data = new LoggingConfig().DefaultLoggingConfig(guild);
            try
            {
                await context.LoggingConfigs.AddAsync(data);
                await context.SaveChangesAsync();
                return await context.LoggingConfigs.FindAsync(guild);
            }
            catch
            {
                return data;
            }
        }

        public static async Task<SuggestionConfig> GetOrCreateSuggestionConfigAsync(this DbService context,
            IGuild guild) =>
            await GetOrCreateSuggestionConfigAsync(context, guild.Id);

        public static async Task<SuggestionConfig> GetOrCreateSuggestionConfigAsync(this DbService context, ulong guild)
        {
            var response = await context.SuggestionConfigs.FindAsync(guild);
            if (response != null) return response;

            var data = new SuggestionConfig().DefaultSuggestionConfig(guild);
            try
            {
                await context.SuggestionConfigs.AddAsync(data);
                await context.SaveChangesAsync();
                return await context.SuggestionConfigs.FindAsync(guild);
            }
            catch
            {
                return data;
            }
        }

        public static async Task<WelcomeConfig> GetOrCreateWelcomeConfigAsync(this DbService context, IGuild guild) =>
            await GetOrCreateWelcomeConfigAsync(context, guild.Id);

        public static async Task<WelcomeConfig> GetOrCreateWelcomeConfigAsync(this DbService context, ulong guild)
        {
            var response = await context.WelcomeConfigs.FindAsync(guild);
            if (response != null) return response;

            var data = new WelcomeConfig().DefaultWelcomeConfig(guild);
            try
            {
                await context.WelcomeConfigs.AddAsync(data);
                await context.SaveChangesAsync();
                return await context.WelcomeConfigs.FindAsync(guild);
            }
            catch
            {
                return data;
            }
        }

        public static async Task<DropConfig> GetOrCreateDropConfig(this DbService context, IGuild guild) =>
            await GetOrCreateDropConfig(context, guild.Id).ConfigureAwait(false);

        public static async Task<DropConfig> GetOrCreateDropConfig(this DbService context, ulong guildId)
        {
            var response = await context.DropConfigs.FindAsync(guildId);
            if (response != null) return response;
            var data = new DropConfig().DefaultDropConfig(guildId);
            try
            {
                await context.DropConfigs.AddAsync(data);
                await context.SaveChangesAsync();
                return await context.DropConfigs.FindAsync(guildId);
            }
            catch
            {
                return data;
            }
        }
    }
}