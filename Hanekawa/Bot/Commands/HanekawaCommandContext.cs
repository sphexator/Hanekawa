using System;
using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Bot.Commands
{
    public class HanekawaCommandContext : DiscordCommandContext
    {
        public IMember Member { get; set; }
        public IGuild Guild { get; set; }
        public ITextChannel Channel { get; set; }
        public IServiceScope Scope { get; set; }

        public HanekawaCommandContext(DiscordBotBase bot, IPrefix prefix, IGatewayUserMessage message, IServiceScope serviceScope) : base(bot, prefix, message, serviceScope)
        {
            if (message.Author is not IMember member)
                throw new InvalidOperationException("Bot does not function in dms :)");
            Scope = serviceScope;
            var guild = bot.GetGuild(member.GuildId);
            if(!guild.Channels.TryGetValue(message.ChannelId, out var channel) || channel is not ITextChannel txtChannel) 
                throw new InvalidOperationException("Bot does not function in dms :)");

            Member = member;
            Guild = bot.GetGuild(member.GuildId);
            Channel = txtChannel;
        }
    }
}
