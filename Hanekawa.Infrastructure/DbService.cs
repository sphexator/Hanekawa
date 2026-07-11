using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Services;
using Hanekawa.Entities;
using Hanekawa.Entities.Club;
using Hanekawa.Entities.Configs;
using Hanekawa.Entities.Internals;
using Hanekawa.Entities.Levels;
using Hanekawa.Entities.Users;
using Hanekawa.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Infrastructure;

internal class DbService(DbContextOptions<DbService> options, TenantService tenantService) 
    : DbContext(options), IDbContext
{
    private TenantService TenantService { get; set; } = tenantService;

    public DbSet<Warning> Warnings { get; set; } = null!;
    public DbSet<Log> Logs { get; set; } = null!;
    public DbSet<GuildModerationLog> ModerationLogs { get; set; } = null!;
    public DbSet<GuildConfig> GuildConfigs { get; set; } = null!;
    public DbSet<GuildUser> Users { get; set; } = null!;
    public DbSet<LevelRequirement> LevelRequirements { get; set; } = null!;
    public DbSet<Club> Clubs { get; set; } = null!;
    public DbSet<ClubMember> ClubMembers { get; set; } = null!;
    public DbSet<Item> Items { get; set; } = null!;
    public DbSet<ItemType> ItemTypes { get; set; } = null!;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await base.SaveChangesAsync(cancellationToken);

    public async Task<bool> EnsureDatabaseCreated(CancellationToken cancellationToken = default)
        => await base.Database.EnsureCreatedAsync(cancellationToken);

    public async Task MigrateDatabaseAsync(CancellationToken cancellationToken = default)
        => await base.Database.MigrateAsync(cancellationToken: cancellationToken);

    public DbConnection GetConnection()
    {
        return Database.GetDbConnection();
    }
    public Task<T?> ExecuteQuery<T>(string query, object? param = null, CancellationToken cancellationToken = default)
    {
        return Database.GetDbConnection()
            .QueryFirstOrDefaultAsync<T>(query, param, commandType: CommandType.Text);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IGuildEntity>().HasQueryFilter(e => e.GuildId == TenantService.GuildId);
        
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
            x.HasOne(e => e.LogConfig)
                .WithOne(e => e.GuildConfig)
                .HasForeignKey<LogConfig>(f => f.GuildId)
                .OnDelete(DeleteBehavior.Cascade);
            x.HasOne(e => e.AdminConfig)
                .WithOne(e => e.GuildConfig)
                .HasForeignKey<AdminConfig>(f => f.GuildId)
                .OnDelete(DeleteBehavior.Cascade);
            x.HasOne(e => e.DropConfig)
                .WithOne(e => e.GuildConfig)
                .HasForeignKey<DropConfig>(f => f.GuildId)
                .OnDelete(DeleteBehavior.Cascade);
            x.HasOne(e => e.CurrencyConfig)
                .WithOne(e => e.GuildConfig)
                .HasForeignKey<CurrencyConfig>(f => f.GuildId)
                .OnDelete(DeleteBehavior.Cascade);
            x.HasOne(e => e.BoostConfig)
                .WithOne(e => e.GuildConfig)
                .HasForeignKey<BoostConfig>(f => f.GuildId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GuildUser>(x =>
        {
            x.HasKey(e => new { e.GuildId, e.Id });
            x.HasOne(e => e.User)
                .WithMany(e => e.GuildUsers)
                .HasForeignKey(e => e.Id)
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

        modelBuilder.Entity<Club>(x =>
        {
            x.HasKey(e => new { e.GuildId, e.Name });
            x.HasIndex(e => new { e.GuildId, e.Name }).IsUnique();
            x.Property(e => e.Name).HasMaxLength(100);
            x.Property(e => e.Description).HasMaxLength(1000);
            x.HasMany(e => e.Members)
                .WithOne(e => e.Club)
                .HasForeignKey(e => new { e.GuildId, e.ClubName })
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ClubMember>(x =>
        {
            x.HasKey(e => new { e.GuildId, e.ClubName, e.UserId });
            x.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => new { e.GuildId, e.UserId })
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>(x =>
        {
            x.HasKey(e => e.Id);
            x.HasMany(e => e.Inventory)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Inventory>(x =>
        {
            x.HasKey(e => new { e.UserId, e.ItemId });
            x.HasOne(e => e.Item)
                .WithMany()
                .HasForeignKey(e => e.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Item>(x =>
        {
            x.HasKey(e => e.Id);
            x.HasOne(e => e.Type)
                .WithMany(e => e.Items)
                .HasForeignKey(e => e.TypeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ItemType>(x =>
        {
            x.HasKey(e => e.Id);
            x.Property(e => e.Name).HasMaxLength(50);
        });
    }

    public Task<int> ExecuteCommand(string command, object? param = null, CancellationToken cancellationToken = default)
    {
        return Database.GetDbConnection()
            .ExecuteAsync(command, param, commandType: CommandType.Text);
    }
}