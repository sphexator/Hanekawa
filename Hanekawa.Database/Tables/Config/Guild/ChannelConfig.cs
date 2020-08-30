using System.Collections.Generic;

namespace Hanekawa.Database.Tables.Config.Guild
{
    public class ChannelConfig
    {
        public ulong GuildId { get; set; }
        public ulong? ReportChannel { get; set; } = null;
        public ulong? EventChannel { get; set; } = null;
        public ulong? EventSchedulerChannel { get; set; } = null;
        public ulong? ModChannel { get; set; } = null;
        public ulong? DesignChannel { get; set; } = null;
        public ulong? QuestionAndAnswerChannel { get; set; } = null;

        public ulong? SelfAssignableChannel { get; set; } = null;
        public List<ulong> SelfAssignableMessages { get; set; } = null;
    }
}