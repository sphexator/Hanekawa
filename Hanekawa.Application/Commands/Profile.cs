using System.IO;
using System.Threading.Tasks;
using Hanekawa.Interfaces.Commands;

namespace Hanekawa.Application.Commands
{
    public class Profile : IProfileCommands
    {
        public ValueTask<MemoryStream> CreateProfileAsync(ulong guildId, ulong userId)
        {
            throw new System.NotImplementedException();
        }
    }
}