using Hanekawa.Database.Tables.Account.ShipGame;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Hanekawa.Database
{
    public partial class DbService
    {
        public DbSet<GameClass> GameClasses { get; set; }
        public DbSet<GameConfig> GameConfigs { get; set; }
        public DbSet<GameEnemy> GameEnemies { get; set; }
        
        private static void GameBuilder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GameClass>(x =>
            {
                x.HasKey(e => e.Id);
                x.Property(e => e.Id).ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<GameConfig>(x =>
            {
                x.HasKey(e => e.Id);
                x.Property(e => e.Id).ValueGeneratedOnAdd();
            });
            modelBuilder.Entity<GameEnemy>(x =>
            {
                x.HasKey(e => e.Id);
                x.Property(e => e.Id).ValueGeneratedOnAdd();
            });
        }
    }
}