using System;

namespace Hanekawa.Patreon.Entities.old
{
    public class Pledge
    {
        public int AmountCents { get; set; }
        public DateTime CreatedAt { get; set; }
        public User Creator { get; set; }
        public DateTime? DeclinedSince { get; set; }
        public bool HasShippingAddress { get; set; }
        public bool IsPaused { get; set; }
        public User Patron { get; set; }
        public bool PatronPaysFees { get; set; }
        public int PledgeCapCents { get; set; }
        public Reward Reward { get; set; }

        //Optional properties.  Will be null if not requested
        public int TotalHistoricalAmountCents { get; set; }
    }
}