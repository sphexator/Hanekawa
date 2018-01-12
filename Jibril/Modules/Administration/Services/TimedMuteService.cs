using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Jibril.Common.Collections;
using Jibril.Extensions;

namespace Jibril.Modules.Administration.Services
{
    public enum MuteType
    {
        Voice,
        Chat,
        All
    }
    public class TimedMuteService
    {
        public ConcurrentDictionary<ulong, string> GuildMuteRoles { get; }
        public ConcurrentDictionary<ulong, ConcurrentHashSet<ulong>> MutedUsers { get; }
        public ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, Timer>> UnmuteTimers { get; }
        = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, Timer>>();
    }
}