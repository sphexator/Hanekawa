using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Humanizer;
using Jibril.Common.Collections;
using Jibril.Data.Variables;
using Jibril.Services.Common;
using Jibril.Services.Logging;

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
        public const string GuildMuteRole = "Mute";
        public List<ulong> MutedUsers;
        public ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, Timer>> UnmuteTimers { get; }
            = new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, Timer>>();

        public event Action<IGuildUser, MuteType> UserMuted = delegate { };
        public event Action<IGuildUser, MuteType> UserUnmuted = delegate { };

        private static readonly OverwritePermissions DenyOverwrite = new OverwritePermissions(addReactions: PermValue.Deny, sendMessages: PermValue.Deny, attachFiles: PermValue.Deny);

        private readonly DiscordSocketClient _client;
        public const ulong GuildId = 339370914724446208;

        public TimedMuteService(DiscordSocketClient client)
        {
            _client = client;
            MutedUsers = AdminDb.GetMutedUsersids();
            var users = AdminDb.GetMutedUsers();
            foreach (var x in users)
            {
                TimeSpan after;
                if (x.Timer - TimeSpan.FromMinutes(2) <= DateTime.Now)
                {
                    after = TimeSpan.FromMinutes(2);
                }
                else
                {
                    after = x.Timer - DateTime.Now;
                }
                StartUnmuteTimer(x.Userid, after);
            }

            //_client.UserJoined += Client_UserJoined;
        }
        private Task Client_UserJoined(IGuildUser usr)
        {
            try
            {
                var muted = AdminDb.GetMutedUsersid(usr.Id);

                if (muted == null) return Task.CompletedTask;
                //var _ = Task.Run(() => MuteUser(usr).ConfigureAwait(false));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return Task.CompletedTask;
        }
        public async Task MuteUser(IGuildUser usr, MuteType type = MuteType.All)
        {
            await usr.ModifyAsync(x => x.Mute = true).ConfigureAwait(false);
            var muteRole = await GetMuteRole(usr.Guild);
            if (!usr.RoleIds.Contains(muteRole.Id))
                await usr.AddRoleAsync(muteRole).ConfigureAwait(false);
            try
            {
                StopUnmuteTimer(usr.Id);
            }
            catch
            {
                //ignore
            }
            UserMuted(usr, MuteType.All);
        }

        public async Task UnmuteUser(IGuildUser usr, MuteType type = MuteType.All)
        {
            StopUnmuteTimer(usr.Id);
            try { await usr.ModifyAsync(x => x.Mute = false).ConfigureAwait(false); } catch { /*ignore*/ }
            try { await usr.RemoveRoleAsync(await GetMuteRole(usr.Guild)).ConfigureAwait(false); } catch { /*ignore*/ }
            AdminDb.RemoveTimedMute(GuildId, usr.Id);
            UserUnmuted(usr, MuteType.All);
        }

        public async Task LogUnmute(SocketGuild guild, IGuildUser user)
        {
            var author = new EmbedAuthorBuilder
            {
                IconUrl = user.GetAvatarUrl(),
                Name = $"{ActionType.Ungagged}|{user.Username}#{user.DiscriminatorValue}"
            };
            var footer = new EmbedFooterBuilder
            {
                Text = $"ID:{user.Id}|{DateTime.UtcNow.Humanize()}"
            };
            var embed = new EmbedBuilder
            {
                Color = new Color(Colours.OKColour),
                Author = author,
                Footer = footer
            };
            embed.AddField(x =>
            {
                x.Name = "User";
                x.Value = $"{user.Mention}";
                x.IsInline = true;
            });
            var log = guild.GetTextChannel(339381104534355970);
            await log.SendMessageAsync("", false, embed.Build());
        }

        public async Task<IRole> GetMuteRole(IGuild guild)
        {
            const string defaultMuteRoleName = "Mute";

            var muteRole = guild.Roles.FirstOrDefault(r => r.Name == GuildMuteRole);
            if (muteRole == null)
            {

                //if it doesn't exist, create it 
                try { muteRole = await guild.CreateRoleAsync(GuildMuteRole, GuildPermissions.None).ConfigureAwait(false); }
                catch
                {
                    //if creations fails,  maybe the name is not correct, find default one, if doesn't work, create default one
                    muteRole = guild.Roles.FirstOrDefault(r => r.Name == GuildMuteRole) ??
                               await guild.CreateRoleAsync(defaultMuteRoleName, GuildPermissions.None).ConfigureAwait(false);
                }
            }

            foreach (var toOverwrite in (await guild.GetTextChannelsAsync()))
            {
                try
                {
                    if (!toOverwrite.PermissionOverwrites.Select(x => x.Permissions).Contains(DenyOverwrite))
                    {
                        await toOverwrite.AddPermissionOverwriteAsync(muteRole, DenyOverwrite)
                            .ConfigureAwait(false);

                        await Task.Delay(200).ConfigureAwait(false);
                    }
                }
                catch
                {
                    // ignored
                }
            }

            return muteRole;
        }

        public async Task TimedMute(IGuildUser user, TimeSpan after)
        {
            await MuteUser(user).ConfigureAwait(false); // mute the user. This will also remove any previous unmute timers

            var unmuateAt = DateTime.UtcNow + after;
            AdminDb.AddTimedMute(GuildId, user.Id, unmuateAt);

            StartUnmuteTimer(user.Id, after); // start the timer
        }

        public void StartUnmuteTimer(ulong userId, TimeSpan after)
        {
            //load the unmute timers for this guild
            var userUnmuteTimers = UnmuteTimers.GetOrAdd(GuildId, new ConcurrentDictionary<ulong, Timer>());

            //unmute timer to be added
            var toAdd = new Timer(async _ =>
            {
                try
                {
                    var guild = _client.GetGuild(GuildId); // load the guild
                    // unmute the user, this will also remove the timer from the db
                    await UnmuteUser(guild.GetUser(userId)).ConfigureAwait(false);
                    await LogUnmute(guild, guild.GetUser(userId)).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    RemoveUnmuteTimerFromDb(userId); // if unmute errored, just remove unmute from db
                    Console.Write("Couldn't unmute user {0} in guild {1}", userId, GuildId);
                    Console.Write(ex);
                }
            }, null, after, Timeout.InfiniteTimeSpan);

            //add it, or stop the old one and add this one
            userUnmuteTimers.AddOrUpdate(userId, (key) => toAdd, (key, old) =>
            {
                old.Change(Timeout.Infinite, Timeout.Infinite);
                return toAdd;
            });
        }

        public void StopUnmuteTimer(ulong userId)
        {
            if (!UnmuteTimers.TryGetValue(GuildId, out ConcurrentDictionary<ulong, Timer> userUnmuteTimers)) return;

            if (userUnmuteTimers.TryRemove(userId, out Timer removed))
            {
                removed.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        private void RemoveUnmuteTimerFromDb(ulong userId)
        {
            AdminDb.RemoveTimedMute(GuildId ,userId);
        }
    }
}