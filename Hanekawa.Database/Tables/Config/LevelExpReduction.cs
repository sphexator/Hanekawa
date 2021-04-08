using Hanekawa.Database.Entities;

namespace Hanekawa.Database.Tables.Config
{
    public class LevelExpReduction
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public ChannelType ChannelType { get; set; } = ChannelType.Text;
    }
}