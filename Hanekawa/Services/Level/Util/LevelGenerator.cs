using Hanekawa.Entities.Interfaces;

namespace Hanekawa.Services.Level.Util
{
    public class LevelGenerator : IHanaService
    {
        public int GetServerLevelRequirement(int currentLevel) => 3 * currentLevel * currentLevel + 150;
        public int GetGlobalLevelRequirement(int currentLevel) => 50 * currentLevel * currentLevel + 300;
    }
}
