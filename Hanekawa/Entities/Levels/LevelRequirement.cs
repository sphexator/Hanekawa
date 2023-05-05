namespace Hanekawa.Entities.Levels;

public class LevelRequirement
{
    /// <summary>
    /// Level
    /// </summary>
    public int Level { get; set; }
    /// <summary>
    /// Requirement to hit this level
    /// </summary>
    public int Experience { get; set; }
}