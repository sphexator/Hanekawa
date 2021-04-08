using System;
using Disqord.Bot;
using Disqord.Gateway;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Bot.Commands
{
    public class HanekawaCommandContext : DiscordCommandContext
    {
        public CachedMember User { get; set; }
        public CachedGuild Guild { get; set; }
        public CachedTextChannel Channel { get; set; }
        public IServiceScope Scope { get; set; }
        // TODO: add colour service
        
        public HanekawaCommandContext(DiscordBotBase bot, IPrefix prefix, IGatewayUserMessage message, IServiceScope serviceScope) : base(bot, prefix, message, serviceScope)
        {
            if (!(message.Author is CachedMember member))
                throw new InvalidOperationException("Bot does not function in dms :)");
            var guild = bot.GetGuild(member.GuildId);
            if(!guild.Channels.TryGetValue(message.ChannelId, out var channel) || !(channel is CachedTextChannel txtChannel)) 
                throw new InvalidOperationException("Bot does not function in dms :)");

            User = member;
            Guild = bot.GetGuild(member.GuildId);
            Channel = txtChannel;
        }
    }
}
