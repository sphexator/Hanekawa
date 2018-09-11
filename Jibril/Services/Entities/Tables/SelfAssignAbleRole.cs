namespace Hanekawa.Services.Entities.Tables
{
    public class SelfAssignAbleRole
    {
        public ulong GuildId {get;set;}
        public ulong RoleId {get;set;}
        public bool Exclusive {get;set;}
    }
}