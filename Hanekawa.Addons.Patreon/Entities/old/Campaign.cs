using System.Collections.Generic;

namespace Hanekawa.Addons.Patreon.Entities.old
{
    public class Campaign
    {
        public string About { get; set; }
        public string CreatedAt { get; set; }
        public int CreationCount { get; set; }
        public string CreationName { get; set; }
        public User Creator { get; set; }
        public string DiscordServerId { get; set; }
        public bool DisplayPatronGoals { get; set; }
        public string EarningsVisibility { get; set; }
        public List<Goal> Goal { get; set; }
        public string ImageSmallUrl { get; set; }
        public string ImageUrl { get; set; }

        public bool IsChargedImmediately { get; set; }

        public bool IsMonthly { get; set; }
        public bool IsNsfw { get; set; }
        public bool IsPlural { get; set; }
        public string MainVideoEmbed { get; set; }
        public string MainVideoUrl { get; set; }
        public string OneLiner { get; set; }

        public int OutstandingPaymentAmountCents { get; set; }

        public int PatronCount { get; set; }
        public string PayPerName { get; set; }
        public List<Pledge> Pledges { get; set; }
        public int PledgeSum { get; set; }
        public string PledgeUrl { get; set; }
        public string PublishedAt { get; set; }
        public List<Reward> Rewards { get; set; }
        public string Summary { get; set; }
        public string ThanksEmbed { get; set; }
        public string ThanksMsg { get; set; }
        public string ThanksVideoUrl { get; set; }
    }
}