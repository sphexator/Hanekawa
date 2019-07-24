using System;
using System.Threading.Tasks;
using Discord;
using Hanekawa.Shared.Command;
using Qmmands;

namespace Hanekawa.Bot.TypeReaders
{
    public class EmoteTypeReader : HanekawaTypeParser<Emote>
    {
        public override ValueTask<TypeParserResult<Emote>> ParseAsync(Parameter parameter, string value,
            HanekawaContext context, IServiceProvider provider) =>
            Emote.TryParse(value, out var emote)
                ? TypeParserResult<Emote>.Successful(emote)
                : TypeParserResult<Emote>.Unsuccessful("Failed to parse into a emote");
    }
}