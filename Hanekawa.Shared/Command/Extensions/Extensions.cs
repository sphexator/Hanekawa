using System.Globalization;
using Disqord;

namespace Hanekawa.Shared.Command
{
    public static class Extensions
    {
        public static bool HasMentionPrefix(this IMessage message, IUser user, out string prefix, out string parsed)
        {
            var content = message.Content;
            parsed = "";
            prefix = "";
            if (content.Length <= 3 || content[0] != '<' || content[1] != '@')
                return false;

            var endPos = content.IndexOf('>');
            if (endPos == -1) return false;

            if (content.Length < endPos + 2 || content[endPos + 1] != ' ')
                return false;

            if (!TryParseUser(content.Substring(0, endPos + 1), out var userId))
                return false;

            if (userId != user.Id.RawValue) return false;
            parsed = content.Substring(endPos + 2);

            prefix = user.Mention;
            return true;
        }

        public static bool TryParseUser(string text, out ulong userId)
        {
            if (text.Length >= 3 && text[0] == '<' && text[1] == '@' && text[text.Length - 1] == '>')
            {
                if (text.Length >= 4 && text[2] == '!')
                    text = text.Substring(3, text.Length - 4); //<@!123>
                else
                    text = text.Substring(2, text.Length - 3); //<@123>

                if (ulong.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out userId))
                    return true;
            }
            userId = 0;
            return false;
        }
    }
}
