using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord;
using Disqord.Rest;
using Humanizer;

namespace Hanekawa.Extensions
{
    public static class MessageExtension
    {
        public static async Task<List<Snowflake>> FilterMessagesAsync(this ITextChannel channel, int amount = 100,
            IMember filterBy = null)
        {
            var messages = await channel.FetchMessagesAsync(amount);
            return messages.FilterMessages(filterBy);
        }

        public static List<Snowflake> FilterMessages(this IEnumerable<IMessage> messages, IMember filterBy = null)
        {
            var list = new List<Snowflake>();
            foreach (var x in messages)
            {
                if (x.CreatedAt.AddDays(14) < DateTimeOffset.UtcNow) continue;
                if (filterBy != null && x.Author.Id != filterBy.Id) continue;
                var dummy = x.Id;
                list.Add(dummy);
            }

            return list;
        }

        public static LocalMessage Create(this LocalMessageBuilder builder, LocalEmbedBuilder embed, 
            LocalMentionsBuilder mention = null)
        {
            builder.Embed = embed;
            builder.Mentions = mention ?? LocalMentionsBuilder.None;
            return builder.Build();
        }
        
        public static LocalMessage Create(this LocalMessageBuilder builder, string message, Color color, 
            LocalMentionsBuilder mention = null) =>
            builder.CreateWithoutBuild(message, color, mention).Build();
        
        public static LocalEmbed Create(this LocalEmbedBuilder builder, string message, Color color, 
            LocalMentionsBuilder mention = null) =>
            builder.CreateDefaultEmbed(message, color, mention).Build();

        public static LocalMessageBuilder CreateWithoutBuild(this LocalMessageBuilder builder, string message, Color color, 
            LocalMentionsBuilder mention = null)
        {
            builder.Embed = new LocalEmbedBuilder
            {
                Color = color,
                Description = message.Truncate(2000)
            };
            builder.Mentions = mention ?? LocalMentionsBuilder.None;
            return builder;
        }
        
        public static LocalEmbedBuilder CreateDefaultEmbed(this LocalEmbedBuilder builder, string message, Color color, 
            LocalMentionsBuilder mention = null)
        {
            builder.Color = color;
            builder.Description = message.Truncate(2000);
            return builder;
        }

        public static async Task<bool> TryDeleteMessageAsync(this IMessage message)
        {
            try
            {
                await message.DeleteAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> TryDeleteMessagesAsync(this ITextChannel channel, IEnumerable<Snowflake> messageIds)
        {
            try
            {
                await channel.DeleteMessagesAsync(messageIds);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}