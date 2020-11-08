using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Parsers;
using Qmmands;

namespace Hanekawa.Shared.Command.TypeParser
{
    public sealed class MemberTypeParser : TypeParser<CachedMember>
    {
        private readonly CachedMemberTypeParser _cachedMemberParser;

        public MemberTypeParser() => _cachedMemberParser = new CachedMemberTypeParser(StringComparison.OrdinalIgnoreCase);

        public override async ValueTask<TypeParserResult<CachedMember>> ParseAsync(Parameter parameter, string value, CommandContext _)
        {
            var context = (DiscordCommandContext)_;
            var result = await _cachedMemberParser.ParseAsync(parameter, value, context);

            if (result.IsSuccessful) 
                return TypeParserResult<CachedMember>.Successful(result.Value);

            if (!Discord.TryParseUserMention(value, out var id) && !Snowflake.TryParse(value, out id)) 
                return TypeParserResult<CachedMember>.Unsuccessful("User not found");

            IMember member = await context.Guild.GetMemberAsync(id);
            try
            {
                return member == null 
                    ? TypeParserResult<CachedMember>.Unsuccessful("User not found") 
                    : TypeParserResult<CachedMember>.Successful(member as CachedMember);
            }
            catch
            {
                return TypeParserResult<CachedMember>.Unsuccessful("User not found");
            }
        }
    }
}
