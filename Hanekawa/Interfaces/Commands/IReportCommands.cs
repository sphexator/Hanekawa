using System.Threading.Tasks;

namespace Hanekawa.Interfaces.Commands
{
    public interface IReportCommands
    {
        ValueTask ReportAsync(ulong reporterId, ulong userId, string report);
    }
}