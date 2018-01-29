using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Jibril.Services.Interactivity
{
    public class I_am_infamous
    {
        private readonly DiscordSocketClient _client;

        private readonly uint _invokeLimit;
        private readonly TimeSpan _invokeLimitPeriod;
        private readonly Dictionary<ulong, MessageTimeout> _invokeTracker = new Dictionary<ulong, MessageTimeout>();

        public I_am_infamous(DiscordSocketClient client)
        {
            _client = client;

            _client.MessageReceived += MessageCounter;
        }

        private Task MessageCounter(SocketMessage msg)
        {
            var _ = Task.Run(() =>
            {
                if (!(msg is SocketUserMessage message)) return;
                if (message.Source != MessageSource.User) return;
                var cd = CheckCooldownAsync(msg.Author);
                if (cd) return;
                DatabaseService.AddMessageCounter(msg.Author);
            });
            return Task.CompletedTask;
        }

        private bool CheckCooldownAsync(SocketUser user)
        {
            var now = DateTime.UtcNow;
            var timeout = _invokeTracker.TryGetValue(user.Id, out var t)
                          && now - t.FirstInvoke < _invokeLimitPeriod
                ? t
                : new MessageTimeout(now);

            timeout.TimesInvoked++;

            if (timeout.TimesInvoked <= _invokeLimit)
            {
                _invokeTracker[user.Id] = timeout;
                return false;
            }

            return true;
        }

        private class MessageTimeout
        {
            public MessageTimeout(DateTime timeStarted)
            {
                FirstInvoke = timeStarted;
            }

            public uint TimesInvoked { get; set; }
            public DateTime FirstInvoke { get; }
        }
    }
}
