using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Shared.Command;
using Qmmands;

namespace Hanekawa.Bot.TypeReaders
{
    public class RoleParser : HanekawaTypeParser<SocketRole>
    {
        public override ValueTask<TypeParserResult<SocketRole>> ParseAsync(Parameter parameter, string value,
            HanekawaContext context, IServiceProvider provider)
        {
            if (MentionUtils.TryParseRole(value, out var id))
            {
                var role = context.Guild.GetRole(id);
                return role != null
                    ? TypeParserResult<SocketRole>.Successful(role)
                    : TypeParserResult<SocketRole>.Unsuccessful("Couldn't parse role");
            }

            if (ulong.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out id))
            {
                var role = context.Guild.GetRole(id);
                return role != null
                    ? TypeParserResult<SocketRole>.Successful(role)
                    : TypeParserResult<SocketRole>.Unsuccessful("Couldn't parse role");
            }

            var roleCheck = context.Guild.Roles.FirstOrDefault(x =>
                string.Equals(x.Name, value, StringComparison.CurrentCultureIgnoreCase));
            return roleCheck != null
                ? TypeParserResult<SocketRole>.Successful(roleCheck)
                : TypeParserResult<SocketRole>.Unsuccessful("Couldn't parse role");
        }
    }
}