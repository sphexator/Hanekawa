using System.Threading.Tasks;

namespace Hanekawa.Interfaces.Commands
{
    public interface ISuggestionCommands : ICommandSettings
    {
        ValueTask UpdateSignAsync();
    }
}