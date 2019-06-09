using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Core;
using Qmmands;

namespace Hanekawa.Bot.TypeReaders
{
    public class CategoryParser : HanekawaTypeParser<SocketCategoryChannel>
    {
        public override ValueTask<TypeParserResult<SocketCategoryChannel>> ParseAsync(Parameter parameter, string value, HanekawaContext context, IServiceProvider provider)
        {
            if (ulong.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var id))
            {
                var channelId = context.Guild.GetCategoryChannel(id);
                return channelId == null
                    ? TypeParserResult<SocketCategoryChannel>.Unsuccessful("No category found")
                    : TypeParserResult<SocketCategoryChannel>.Successful(channelId);
            }

            var channel = context.Guild.CategoryChannels.FirstOrDefault(x => x.Name == value);
            return channel == null 
                ? TypeParserResult<SocketCategoryChannel>.Unsuccessful("No category found") 
                : TypeParserResult<SocketCategoryChannel>.Successful(channel);
        }
    }
}
