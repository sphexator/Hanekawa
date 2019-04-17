using Hanekawa.Patreon.Entities.old;

namespace Hanekawa.Patreon.Entities
{
    public class PledgeReturn
    {
        public old.Pledge Pledges { get; set; }
        public User Users { get; set; }
    }
}