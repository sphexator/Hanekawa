using System;

namespace Hanekawa.Patreon.Entities.old
{
    public class User
    {
        public DateTime Created { get; set; }
        public ulong DiscordId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public bool IsEmailVerified { get; set; }
        public string Url { get; set; }
        public string Vanity { get; set; }
    }
}