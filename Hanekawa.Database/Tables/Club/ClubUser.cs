using System;
using Disqord;
using Hanekawa.Database.Entities;

namespace Hanekawa.Database.Tables.Club
{
    public class ClubUser
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Snowflake GuildId { get; set; }
        public Snowflake UserId { get; set; }
        public ClubRank Rank { get; set; } = ClubRank.Member;
        public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;

        public Guid ClubId { get; set; }
        public Club Club { get; set; }
    }
}