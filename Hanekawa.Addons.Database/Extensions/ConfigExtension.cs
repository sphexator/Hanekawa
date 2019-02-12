using System.Threading.Tasks;
using Discord;
using Hanekawa.Addons.Database.Tables.Config;
using Hanekawa.Addons.Database.Tables.Config.Guild;

namespace Hanekawa.Addons.Database.Extensions
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
            await context.GuildConfigs.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.GuildConfigs.FindAsync(guild);
        }

        public static GuildConfig GetOrCreateGuildConfig(this DbService context, IGuild guild) =>
            GetOrCreateGuildConfig(context, guild.Id);

        public static GuildConfig GetOrCreateGuildConfig(this DbService context, ulong guild)
        {
            var response = context.GuildConfigs.Find(guild);
            if (response != null) return response;

            var data = new GuildConfig().DefaultGuildConfig(guild);
            context.GuildConfigs.Add(data);
            context.SaveChanges();
            return context.GuildConfigs.Find(guild);
        }

        public static async Task<AdminConfig> GetOrCreateAdminConfigAsync(this DbService context, IGuild guild) =>
            await GetOrCreateAdminConfigAsync(context, guild.Id);

        public static async Task<AdminConfig> GetOrCreateAdminConfigAsync(this DbService context, ulong guild)
        {
            var response = await context.AdminConfigs.FindAsync(guild);
            if (response != null) return response;

            var data = new AdminConfig().DefaultAdminConfig(guild);
            await context.AdminConfigs.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.AdminConfigs.FindAsync(guild);
        }

        public static async Task<BoardConfig> GetOrCreateBoardConfigAsync(this DbService context, IGuild guild) =>
            await GetOrCreateBoardConfigAsync(context, guild.Id);

        public static async Task<BoardConfig> GetOrCreateBoardConfigAsync(this DbService context, ulong guild)
        {
            var response = await context.BoardConfigs.FindAsync(guild);
            if (response != null) return response;

            var data = new BoardConfig().DefaultBoardConfig(guild);
            await context.BoardConfigs.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.BoardConfigs.FindAsync(guild);
        }

        public static async Task<ChannelConfig> GetOrCreateChannelConfigAsync(this DbService context, IGuild guild) =>
            await GetOrCreateChannelConfigAsync(context, guild.Id);

        public static async Task<ChannelConfig> GetOrCreateChannelConfigAsync(this DbService context, ulong guild)
        {
            var response = await context.ChannelConfigs.FindAsync(guild);
            if (response != null) return response;

            var data = new ChannelConfig().DefaultChannelConfig(guild);
            await context.ChannelConfigs.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.ChannelConfigs.FindAsync(guild);
        }

        public static async Task<ClubConfig> GetOrCreateClubConfigAsync(this DbService context, IGuild guild) =>
            await GetOrCreateClubConfigAsync(context, guild.Id);

        public static async Task<ClubConfig> GetOrCreateClubConfigAsync(this DbService context, ulong guild)
        {
            var response = await context.ClubConfigs.FindAsync(guild);
            if (response != null) return response;

            var data = new ClubConfig().DefaultClubConfig(guild);
            await context.ClubConfigs.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.ClubConfigs.FindAsync(guild);
        }

        public static async Task<CurrencyConfig> GetOrCreateCurrencyConfigAsync(this DbService context, IGuild guild) =>
            await GetOrCreateCurrencyConfigAsync(context, guild.Id);

        public static async Task<CurrencyConfig> GetOrCreateCurrencyConfigAsync(this DbService context, ulong guild)
        {
            var response = await context.CurrencyConfigs.FindAsync(guild);
            if (response != null) return response;

            var data = new CurrencyConfig().DefaultCurrencyConfig(guild);
            await context.CurrencyConfigs.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.CurrencyConfigs.FindAsync(guild);
        }

        public static async Task<LevelConfig> GetOrCreateLevelConfigAsync(this DbService context, IGuild guild) =>
            await GetOrCreateLevelConfigAsync(context, guild.Id);

        public static async Task<LevelConfig> GetOrCreateLevelConfigAsync(this DbService context, ulong guild)
        {
            var response = await context.LevelConfigs.FindAsync(guild);
            if (response != null) return response;

            var data = new LevelConfig().DefaultLevelConfig(guild);
            await context.LevelConfigs.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.LevelConfigs.FindAsync(guild);
        }

        public static async Task<LoggingConfig> GetOrCreateLoggingConfigAsync(this DbService context, IGuild guild) =>
            await GetOrCreateLoggingConfigAsync(context, guild.Id);

        public static async Task<LoggingConfig> GetOrCreateLoggingConfigAsync(this DbService context, ulong guild)
        {
            var response = await context.LoggingConfigs.FindAsync(guild);
            if (response != null) return response;

            var data = new LoggingConfig().DefaultLoggingConfig(guild);
            await context.LoggingConfigs.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.LoggingConfigs.FindAsync(guild);
        }

        public static async Task<SuggestionConfig> GetOrCreateSuggestionConfigAsync(this DbService context, IGuild guild) =>
            await GetOrCreateSuggestionConfigAsync(context, guild.Id);

        public static async Task<SuggestionConfig> GetOrCreateSuggestionConfigAsync(this DbService context, ulong guild)
        {
            var response = await context.SuggestionConfigs.FindAsync(guild);
            if (response != null) return response;

            var data = new SuggestionConfig().DefaultSuggestionConfig(guild);
            await context.SuggestionConfigs.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.SuggestionConfigs.FindAsync(guild);
        }

        public static async Task<WelcomeConfig> GetOrCreateWelcomeConfigAsync(this DbService context, IGuild guild) =>
            await GetOrCreateWelcomeConfigAsync(context, guild.Id);

        public static async Task<WelcomeConfig> GetOrCreateWelcomeConfigAsync(this DbService context, ulong guild)
        {
            var response = await context.WelcomeConfigs.FindAsync(guild);
            if (response != null) return response;

            var data = new WelcomeConfig().DefaultWelcomeConfig(guild);
            await context.WelcomeConfigs.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.WelcomeConfigs.FindAsync(guild);
        }
    }
}