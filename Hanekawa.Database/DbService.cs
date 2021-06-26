using Disqord;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables;
using Hanekawa.Database.Tables.Administration;
using Hanekawa.Database.Tables.AutoMessage;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Database.Tables.Config.Guild;
using Hanekawa.Database.Tables.Internal;
using Hanekawa.Database.Tables.Quote;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Hanekawa.Database
{
    public partial class DbService : DbContext
    { 
        public DbService(DbContextOptions<DbService> options) : base(options) { }

        // Voice Role
        public DbSet<VoiceRoles> VoiceRoles { get; set; }
        
        public DbSet<AutoMessage> AutoMessages { get; set; }

        // Quote
        public DbSet<Quote> Quotes { get; set; }
        
        // Internal
        public virtual DbSet<Log> Logs { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseValueConverterForType<Snowflake>(
                new ValueConverter<Snowflake, ulong>(key => key.RawValue, 
                    value => new Snowflake(value)));
            
            modelBuilder.UseValueConverterForType<Snowflake?>(
                new ValueConverter<Snowflake?, ulong>(key => key.Value.RawValue, 
                    value => new Snowflake(value)));
            
            AccountBuilder(modelBuilder);
            AchievementBuilder(modelBuilder);
            AdministrationBuilder(modelBuilder);
            AdvertisementBuilder(modelBuilder);
            BoardBuilder(modelBuilder);
            ClubBuilder(modelBuilder);
            ConfigBuilder(modelBuilder);
            DropBuilder(modelBuilder);
            GameBuilder(modelBuilder);
            GiveawayBuilder(modelBuilder);
            HungerGameBuilder(modelBuilder);
            InventoryStoreBuilder(modelBuilder);
            MvpBuilder(modelBuilder);
            ReportBuilder(modelBuilder);
            SelfAssignableBuilder(modelBuilder);
            SuggestionBuilder(modelBuilder);
            WelcomeBuilder(modelBuilder);
            
            modelBuilder.Entity<Log>(x =>
            {
                x.HasKey(e => e.Id);
                x.Property(e => e.Id).ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<Blacklist>(x =>
            {
                x.HasKey(e => e.GuildId);
            });
            modelBuilder.Entity<VoiceRoles>(x =>
            {
                x.HasKey(e => new { e.GuildId, e.VoiceId });
            });
            modelBuilder.Entity<AutoMessage>(x =>
            {
                x.HasKey(e => new { e.GuildId, e.Name });
            });
            modelBuilder.Entity<Quote>(x =>
            {
                x.HasKey(e => new { e.GuildId, e.Key });
            });
        }
    }
}