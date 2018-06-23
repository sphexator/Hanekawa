using System;
using Discord;
using Discord.WebSocket;

namespace Jibril.Services.Administration
{
    public class MuteService
    {
        public enum MuteType
        {
            Voice,
            Chat,
            All
        }

        private readonly DiscordSocketClient _client;
        
        public event Action<IGuildUser, MuteType> UserMuted = delegate {  };
        public event Action<IGuildUser, MuteType> UserUnmuted = delegate { };

        private static readonly OverwritePermissions DenyOverwrite = new OverwritePermissions(addReactions: PermValue.Deny, sendMessages: PermValue.Deny, attachFiles: PermValue.Deny);

        public class MuteService
        {

        }
    }
}
