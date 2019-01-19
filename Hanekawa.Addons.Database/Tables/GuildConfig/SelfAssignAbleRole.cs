namespace Hanekawa.Addons.Database.Tables.GuildConfig
{
    public class SelfAssignAbleRole
    {
        public ulong GuildId { get; set; }
        public ulong RoleId { get; set; }
        public bool Exclusive { get; set; } = false;
    }
}
