using System;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Prefixes;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Shared.Command
{
    public class HanekawaCommandContext : DiscordCommandContext
    {
        public IServiceScope Scope { get; }
        public ColourService Colour { get; }
        public new DiscordBot Bot { get; }
        public new CachedMember Member { get; }
        public new CachedTextChannel Channel { get; }

        public HanekawaCommandContext(IServiceScope scope, DiscordBot bot, IPrefix prefix, CachedUserMessage message, ColourService colour)
            : base(bot, prefix, message, scope.ServiceProvider)
        {
            if (!(message.Author is CachedMember member && message.Channel is CachedTextChannel channel))
            {
                throw new InvalidOperationException("Bot should not be used in dms");
            }
            Scope = scope;
            Bot = bot;
            Member = member;
            Channel = channel;
            Colour = colour;
        }
    }
}