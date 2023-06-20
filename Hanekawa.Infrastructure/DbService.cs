using System.Threading.Tasks;
using Hanekawa.Application.Interfaces;
using Hanekawa.Entities;
using Hanekawa.Entities.Configs;
using Hanekawa.Entities.Internals;
using Hanekawa.Entities.Levels;
using Hanekawa.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Infrastructure
{
    /// <inheritdoc cref="Hanekawa.Application.Interfaces.IDbContext" />
    internal class DbService : DbContext, IDbContext
    {
        public DbService(DbContextOptions<DbService> options) : base(options) { }
        /// <inheritdoc />
        public DbSet<Warning> Warnings { get; set; }
        /// <inheritdoc />
        public DbSet<Log> Logs { get; set; } = null!;
        /// <inheritdoc />
        public DbSet<GuildModerationLog> ModerationLogs { get; set; }
        /// <inheritdoc />
        public DbSet<GuildConfig> GuildConfigs { get; set; } = null!;
        /// <inheritdoc />
        public DbSet<GuildUser> Users { get; set; } = null!;
        /// <inheritdoc />
        public DbSet<LevelRequirement> LevelRequirements { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GuildConfig>(x =>
            {
                x.HasKey(e => e.GuildId);
                x.HasOne(e => e.GreetConfig)
                    .WithOne(e => e.GuildConfig)
                    .HasForeignKey<GreetConfig>(f => f.GuildId)
                    .OnDelete(DeleteBehavior.Cascade);
                x.HasOne(e => e.LevelConfig)
                    .WithOne(e => e.GuildConfig)
                    .HasForeignKey<LevelConfig>(f => f.GuildId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            modelBuilder.Entity<GuildUser>(x =>
            {
                x.HasKey(e => new { e.GuildId, e.UserId });
                x.HasOne(e => e.User)
                    .WithMany(e => e.GuildUsers)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            modelBuilder.Entity<GreetConfig>(x =>
            {
                x.HasMany(e => e.Images)
                    .WithOne(e => e.GreetConfig)
                    .HasForeignKey(e => e.GuildId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<LevelConfig>(x =>
            {
                x.HasMany(e => e.Rewards)
                    .WithOne(e => e.LevelConfig)
                    .HasForeignKey(e => e.GuildId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<LevelRequirement>(x =>
            {
                x.HasKey(e => e.Level);
            });
            modelBuilder.Entity<GuildModerationLog>(x =>
            {
                x.HasKey(e => new { e.GuildId, e.Id });
            });
        }
        /// <inheritdoc />
        public async Task<int> SaveChangesAsync() => await base.SaveChangesAsync();
        /// <inheritdoc />
        public async Task<bool> EnsureDatabaseCreated() => await base.Database.EnsureCreatedAsync();
        /// <inheritdoc />
        public async Task MigrateDatabaseAsync() => await base.Database.MigrateAsync();
    }
}