using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hanekawa.Database;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Interfaces;
using Qmmands;

namespace Hanekawa.Bot.Preconditions
{
    public class RequiredMusicChannel : HanekawaAttribute, INService
    {
        private readonly ConcurrentDictionary<ulong, ulong> _musicChannels = new ConcurrentDictionary<ulong, ulong>();

        public RequiredMusicChannel()
        {
            using (var db = new DbService())
            {
                foreach (var x in db.MusicConfigs)
                    if (x.TextChId.HasValue)
                        _musicChannels.TryAdd(x.GuildId, x.TextChId.Value);
            }
        }

        public override ValueTask<CheckResult> CheckAsync(HanekawaContext context, IServiceProvider provider)
        {
            if (context.User.GuildPermissions.ManageGuild) return CheckResult.Successful;

            var check = _musicChannels.TryGetValue(context.Guild.Id, out var channel);
            if (!check) return CheckResult.Unsuccessful("Channel is not set as a music channel");
            return channel != context.Channel.Id
                ? CheckResult.Unsuccessful("Channel is not set as a music channel")
                : CheckResult.Successful;
        }

        public void AddOrRemoveChannel(SocketGuild guild, SocketTextChannel channel = null)
        {
            if (channel == null)
            {
                _musicChannels.TryRemove(guild.Id, out _);
                return;
            }

            _musicChannels.AddOrUpdate(guild.Id, channel.Id, (key, value) => channel.Id);
        }
    }
}