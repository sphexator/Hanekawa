using System.Collections.Generic;

namespace Hanekawa.Addons.Patreon.Entities.old
{
    public class Reward
    {
        public int AmountCents { get; set; }
        public Campaign Campaign { get; set; }
        public string CreatedAt { get; set; }
        public User Creator { get; set; }
        public string Description { get; set; }
        public List<string> DiscordRoleIds { get; set; }
        public string EditedAt { get; set; }
        public string ImageUrl { get; set; }
        public int PatronCount { get; set; }
        public bool Published { get; set; }
        public string PublishedAt { get; set; }
        public float Remaining { get; set; }
        public bool RequiresShipping { get; set; }
        public string Title { get; set; }
        public string UnpublishedAt { get; set; }
        public string Url { get; set; }
        public int UserLimit { get; set; }
    }
}