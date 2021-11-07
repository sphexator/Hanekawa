namespace Hanekawa.Entities.Config.SelfAssign
{
    public class SelfAssignAbleRole
    {
        public ulong GuildId { get; set; }
        public ulong RoleId { get; set; }
        public ulong? EmoteId { get; set; }
        public bool Exclusive { get; set; } = false;

        public string EmoteReactFormat { get; set; }
        public string EmoteMessageFormat { get; set; }
    }
}