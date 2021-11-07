using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hanekawa.Interfaces
{
    public interface IGuildUser : IUser
    {
        ulong GuildId { get; set; }
        string Nick { get; set; }

        ValueTask<ulong> GetRole(ulong roleId);
        ValueTask<List<ulong>> GetRoles();
        ValueTask<IGuild> GetGuild();
    }
}