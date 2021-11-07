using System.Threading.Tasks;

namespace Hanekawa.Interfaces.Commands
{
    public interface ISettings : ICommandSettings
    {
        // TODO: Change to domain colour
        ValueTask SetEmbedColor(ulong guildId, uint color);
    }
}