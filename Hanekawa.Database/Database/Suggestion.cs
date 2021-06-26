using Hanekawa.Database.Tables.Config.Guild;
using Hanekawa.Database.Tables.Moderation;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Hanekawa.Database
{
    public partial class DbService
    {
        public DbSet<Suggestion> Suggestions { get; set; }
        public DbSet<SuggestionConfig> SuggestionConfigs { get; set; }

        private static void SuggestionBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SuggestionConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
            });
            modelBuilder.Entity<Suggestion>(x =>
            {
                x.HasKey(e => new {e.Id, e.GuildId});
            });
        }
    }
}