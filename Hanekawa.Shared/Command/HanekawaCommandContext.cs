using System;
using System.Collections.Generic;
using System.Text;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Prefixes;

namespace Hanekawa.Shared.Command
{
    public class HanekawaCommandContext : DiscordCommandContext
    {
        public IServiceProvider ServiceScope { get; }
        public ColourService Colour { get; }
        public new DiscordBot Bot { get; }
        public new CachedMember Member { get; }
        public new CachedTextChannel Channel { get; }

        public HanekawaCommandContext(IServiceProvider provider, DiscordBot bot, IPrefix prefix, CachedUserMessage message, ColourService colour)
            : base(bot, prefix, message, provider)
        {
            if (!(message.Author is CachedMember member && message.Channel is CachedTextChannel channel))
            {
                throw new InvalidOperationException("Bot should not be used in dms");
            }
            ServiceScope = provider;
            Bot = bot;
            Member = member;
            Channel = channel;
            Colour = colour;
        }
    }
}