using System;
using System.Threading.Tasks;
using Disqord;
using Qmmands;

namespace Hanekawa.Shared.Command.TypeParser
{
    public sealed class CachedGuildChannelTypeParser<TChannel> : HanekawaTypeParser<TChannel> where TChannel : CachedGuildChannel
    {
        public static CachedGuildChannelTypeParser<TChannel> Instance => _instance ?? (_instance = new CachedGuildChannelTypeParser<TChannel>());

        private static CachedGuildChannelTypeParser<TChannel> _instance;

        private CachedGuildChannelTypeParser()
        { }

        public override ValueTask<TypeParserResult<TChannel>> ParseAsync(Parameter parameter, string value, HanekawaContext context, IServiceProvider provider)
        {
            if (context.Guild == null)
                throw new InvalidOperationException("This can only be used in a guild.");

            CachedGuildChannel channel = null;
            if (Discord.TryParseChannelMention(value, out var id) || Snowflake.TryParse(value, out id)) context.Guild.Channels.TryGetValue(id, out channel);
            
            return channel == null
                ? new TypeParserResult<TChannel>("No channel found matching the input.")
                : new TypeParserResult<TChannel>((TChannel)channel);
        }
    }
}