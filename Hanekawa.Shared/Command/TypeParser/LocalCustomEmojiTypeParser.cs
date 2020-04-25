using System;
using System.Threading.Tasks;
using Disqord;
using Qmmands;

namespace Hanekawa.Shared.Command.TypeParser
{
    public class LocalCustomEmojiTypeParser : HanekawaTypeParser<LocalCustomEmoji>
    {
        public static LocalCustomEmojiTypeParser Instance => _instance ?? (_instance = new LocalCustomEmojiTypeParser());

        private static LocalCustomEmojiTypeParser _instance;

        private LocalCustomEmojiTypeParser()
        { }

        public override ValueTask<TypeParserResult<LocalCustomEmoji>> ParseAsync(Parameter parameter, string value, HanekawaContext context, IServiceProvider provider)
            => LocalCustomEmoji.TryParse(value, out var emoji)
                ? TypeParserResult<LocalCustomEmoji>.Successful(emoji)
                : TypeParserResult<LocalCustomEmoji>.Unsuccessful("Invalid custom emoji format.");

    }
}
