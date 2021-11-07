using System.Threading.Tasks;
using Hanekawa.Interfaces.Commands;

namespace Hanekawa.Application.Commands
{
    public class Report : IReportCommands
    {
        public ValueTask ReportAsync(ulong reporterId, ulong userId, string report)
        {
            throw new System.NotImplementedException();
        }
    }
}