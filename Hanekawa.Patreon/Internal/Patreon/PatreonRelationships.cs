using System.Collections.Generic;

namespace Hanekawa.Patreon
{
    public class PatreonRelationships
    {
        private RelationshipItem Address { get; set; }
        private RelationshipItem Card { get; set; }
        private RelationshipItem Creator { get; set; }
        private RelationshipItem Patron { get; set; }
        private RelationshipItem Reward { get; set; }

        private List<PatreonEntity> Goals { get; set; }
        private List<PatreonEntity> Rewards { get; set; }
    }
}