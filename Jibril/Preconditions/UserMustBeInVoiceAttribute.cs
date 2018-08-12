using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Hanekawa.Preconditions
{
    /// <summary> Indicates that this command should only be used while the user is in a voice channel. </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class UserMustBeInVoiceAttribute : PreconditionAttribute
    {
        /// <inheritdoc />
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            return (context.User as IVoiceState) == null ? PreconditionResult.FromError("Command must be invoked while in a voice channel in this guild.") : PreconditionResult.FromSuccess();
        }
    }
}
