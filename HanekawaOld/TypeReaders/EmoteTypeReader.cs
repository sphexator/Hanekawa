using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Hanekawa.TypeReaders
{
    public class EmoteTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input,
            IServiceProvider services) =>
            Task.FromResult(Emote.TryParse(input, out var result)
                ? TypeReaderResult.FromSuccess(result)
                : TypeReaderResult.FromError(CommandError.ParseFailed, "Input could not be parsed into an emote."));
    }
}