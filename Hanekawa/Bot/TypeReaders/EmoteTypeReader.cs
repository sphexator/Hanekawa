using System;
using System.Threading.Tasks;
using Discord;
using Hanekawa.Core;
using Qmmands;

namespace Hanekawa.Bot.TypeReaders
{
    public class EmoteTypeReader : HanekawaTypeParser<Emote>
    {
        public override ValueTask<TypeParserResult<Emote>> ParseAsync(Parameter parameter, string value,
            HanekawaContext context, IServiceProvider provider)
        {
            return Emote.TryParse(value, out var emote)
                ? TypeParserResult<Emote>.Successful(emote)
                : TypeParserResult<Emote>.Unsuccessful("Failed to parse into a emote");
        }
    }
}