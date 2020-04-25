using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Qmmands;

namespace Hanekawa.Shared.Command
{
    public sealed class RequireNsfwAttribute : HanekawaAttribute
    {
        public bool AllowPrivateChannels { get; set; } = true;

        public RequireNsfwAttribute() { }

        public override ValueTask<CheckResult> CheckAsync(HanekawaContext context, IServiceProvider provider) =>
            context.Channel is CachedTextChannel textChannel && textChannel.IsNsfw || AllowPrivateChannels && context.Channel is CachedPrivateChannel
                ? CheckResult.Successful
                : CheckResult.Unsuccessful($"This can only be executed in NSFW{(AllowPrivateChannels ? " or private" : "")} channels.");
    }
}
