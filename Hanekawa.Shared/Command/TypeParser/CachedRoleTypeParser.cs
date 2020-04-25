using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Qmmands;

namespace Hanekawa.Shared.Command.TypeParser
{
    public class CachedRoleTypeParser : HanekawaTypeParser<CachedRole>
    {
        public static CachedRoleTypeParser Instance => _instance ?? (_instance = new CachedRoleTypeParser());

        private static CachedRoleTypeParser _instance;

        private CachedRoleTypeParser()
        { }

        public override ValueTask<TypeParserResult<CachedRole>> ParseAsync(Parameter parameter, string value,
            HanekawaContext context, IServiceProvider provider)
        {

            if (context.Guild == null)
                throw new InvalidOperationException("This can only be used in a guild.");

            CachedRole role = null;
            if (Discord.TryParseRoleMention(value, out var id) || Snowflake.TryParse(value, out id))
                context.Guild.Roles.TryGetValue(id, out role);

            if (role == null)
                role = context.Guild.Roles.Values.FirstOrDefault(x => x.Name == value);

            return role == null
                ? new TypeParserResult<CachedRole>("No role found matching the input.")
                : new TypeParserResult<CachedRole>(role);
        }
    }
}
