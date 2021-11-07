using System.IO;
using System.Threading.Tasks;

namespace Hanekawa.Interfaces.Commands
{
    public interface IProfileCommands
    {
        ValueTask<MemoryStream> CreateProfileAsync(ulong guildId, ulong userId);
    }
}