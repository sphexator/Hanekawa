﻿using Discord;

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

            if (!MentionUtils.TryParseUser(content.Substring(0, endPos + 1), out var userId))
                return false;

            if (userId != user.Id) return false;
            parsed = content.Substring(endPos + 2);

            prefix = user.Mention;
            return true;
        }
    }
}
