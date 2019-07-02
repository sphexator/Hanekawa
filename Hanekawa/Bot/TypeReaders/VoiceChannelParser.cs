using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Shared;
using Qmmands;

namespace Hanekawa.Bot.TypeReaders
{
    public class VoiceChannelParser : HanekawaTypeParser<SocketVoiceChannel>
    {
        public override ValueTask<TypeParserResult<SocketVoiceChannel>> ParseAsync(Parameter parameter, string value,
            HanekawaContext context, IServiceProvider provider)
        {
            if (ulong.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var id))
                return context.Guild.GetChannel(id) is SocketVoiceChannel vcCh
                    ? TypeParserResult<SocketVoiceChannel>.Successful(vcCh)
                    : TypeParserResult<SocketVoiceChannel>.Unsuccessful("Couldn't parse voice channel");

            return context.Guild.VoiceChannels.FirstOrDefault(x => x.Name == value) is SocketVoiceChannel vcCheck
                ? TypeParserResult<SocketVoiceChannel>.Successful(vcCheck)
                : TypeParserResult<SocketVoiceChannel>.Unsuccessful("Couldn't parse voice channel");
        }
    }
}