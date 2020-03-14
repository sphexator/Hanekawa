using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Prefixes;

namespace Hanekawa.Shared.Command
{
    public class HanekawaContext : DiscordCommandContext
    {
        public virtual DiscordBotBase Bot { get; }

        public virtual IPrefix Prefix { get; }

        public virtual CachedUserMessage Message { get; }

        public virtual CachedTextChannel Channel => Message.Channel as CachedTextChannel;

        public virtual CachedUser User => Message.Author;

        public virtual CachedMember Member => User as CachedMember;

        public virtual CachedGuild Guild => Member?.Guild;
        
        public virtual ColourService Colour { get; }

        public HanekawaContext(DiscordBot bot, CachedUserMessage message, IPrefix prefix, ColourService colour, IServiceProvider provider) : base(bot, prefix, message)
        {
            Bot = bot;
            Prefix = prefix;
            Message = message;
            Colour = colour;
        }

        public async Task<IUserMessage> ReplyAsync(string content) =>
            await Channel.SendMessageAsync(null, false, new LocalEmbedBuilder
            {
                Color = Colour.Get(Guild.Id),
                Description = content
            }.Build());

        public async Task<IUserMessage> ReplyAsync(string content, Color color) =>
            await Channel.SendMessageAsync(null, false, new LocalEmbedBuilder
            {
                Color = color,
                Description = content
            }.Build());

        public async Task<IUserMessage> ReplyAsync(LocalEmbedBuilder embed)
        {
            if (embed.Color == null || embed.Color == Color.Purple) embed.Color = Colour.Get(Guild.Id);
            return await Channel.SendMessageAsync(null, false, embed.Build());
        }
    }
}