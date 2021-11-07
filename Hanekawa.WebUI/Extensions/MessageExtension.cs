using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Humanizer;

namespace Hanekawa.WebUI.Extensions
{
    public static class MessageExtension
    {
        public static async Task<List<Snowflake>> FilterMessagesAsync(this CachedMessageGuildChannel channel, int amount = 100,
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
                if (x.CreatedAt().AddDays(14) < DateTimeOffset.UtcNow) continue;
                if (filterBy != null && x.Author.Id != filterBy.Id) continue;
                var dummy = x.Id;
                list.Add(dummy);
            }

            return list;
        }

        public static LocalMessage Create(this LocalMessage builder, LocalEmbed embed,
            LocalAllowedMentions mention = null)
        {
            builder.Embeds = new[] {embed};
            builder.AllowedMentions = mention ?? LocalAllowedMentions.None;
            return builder;
        }

        public static LocalMessage Create(this LocalMessage builder, string message, Color color,
            LocalAllowedMentions mention = null)
        {
            return builder.CreateWithoutBuild(message, color, mention);
        }

        public static LocalEmbed Create(this LocalEmbed builder, string message, Color color,
            LocalAllowedMentions mention = null)
        {
            return builder.CreateDefaultEmbed(message, color, mention);
        }

        public static LocalMessage CreateWithoutBuild(this LocalMessage builder, string message, Color color,
            LocalAllowedMentions mention = null)
        {
            builder.Embeds = new[]
            {
                new LocalEmbed
                {
                    Color = color,
                    Description = message.Truncate(2000)
                }
            };
            builder.AllowedMentions = mention ?? LocalAllowedMentions.None;
            return builder;
        }

        public static LocalEmbed CreateDefaultEmbed(this LocalEmbed builder, string message, Color color,
            LocalAllowedMentions mention = null)
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

        public static async Task<bool> TryDeleteMessagesAsync(this CachedMessageGuildChannel channel,
            IEnumerable<Snowflake> messageIds)
        {
            try
            {
                await (channel as ITextChannel).DeleteMessagesAsync(messageIds);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}