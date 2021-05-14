using System.Threading.Tasks;
using Disqord.Gateway;
using Qmmands;

namespace Hanekawa.Bot.Commands.Preconditions
{
    public class RequireVoiceState : CheckAttribute
    {
        public override ValueTask<CheckResult> CheckAsync(CommandContext _)
        {
            if (_ is not HanekawaCommandContext context) return CheckResult.Failed("Wrong command context.");
            var state = context.Author.GetVoiceState();
            return state is not {ChannelId: { }}
                ? CheckResult.Failed("You need to be in VC to use this command!")
                : CheckResult.Successful;
        }
    }
}