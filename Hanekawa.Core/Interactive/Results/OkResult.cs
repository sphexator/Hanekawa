using Qmmands;

namespace Hanekawa.Core.Interactive.Results
{
    public class OkResult : CommandResult
    {
        public OkResult(string reason = null)
        {
        }

        public override bool IsSuccessful { get; }
    }
}