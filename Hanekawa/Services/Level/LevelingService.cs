using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Entities.Interfaces;
using Hanekawa.Services.Level.Util;
using System;
using System.Linq;
using System.Threading.Tasks;
using Hanekawa.Services.Achievement;
using Hanekawa.Services.Logging;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Services.Level
{
    public class LevelingService : IHanaService, IRequiredService
    {
        private readonly Calculate _calc;
        private readonly DiscordSocketClient _client;
        private readonly LevelHandler _handler;
        private readonly LevelData _data;
        private readonly LevelRoleHandler _roleManager;
        private readonly ExpEvent _expEvent;
        private readonly LogService _log;
        private readonly AchievementManager _achievement;

        public LevelingService(DiscordSocketClient discord, Calculate calc, LevelData data, LevelHandler handler, LevelRoleHandler roleManager, ExpEvent expEvent, LogService log)
        {
            _client = discord;
            _calc = calc;
            _data = data;
            _handler = handler;
            _roleManager = roleManager;
            _expEvent = expEvent;
            _log = log;

            _client.MessageReceived += ServerMessageExpAsync;
            _client.MessageReceived += GlobalMessageExpAsync;
            _client.UserVoiceStateUpdated += VoiceExpAsync;
            _client.UserJoined += GiveRolesBackAsync;

            using (var db = new DbService())
            {
                foreach (var x in db.GuildConfigs)
                {
                    var levelExpEvent = db.LevelExpEvents.Find(x.GuildId);
                    if (levelExpEvent == null)
                    {
                        _handler.AdjustMultiplier(x.GuildId, (int)x.ExpMultiplier);
                    }
                    else if (levelExpEvent.Time - TimeSpan.FromMinutes(2) <= DateTime.UtcNow)
                    {
                        _handler.AdjustMultiplier(x.GuildId, (int)x.ExpMultiplier);
                        var check = db.LevelExpEvents.FirstOrDefault(y => y.GuildId == x.GuildId);
                        if (check == null) continue;
                        db.LevelExpEvents.Remove(check);
                        db.SaveChanges();
                    }
                    else
                    {
                        var after = levelExpEvent.Time - DateTime.UtcNow;
                        _expEvent.ExpEventHandler(db, levelExpEvent.GuildId, (int)levelExpEvent.Multiplier, (int)x.ExpMultiplier, levelExpEvent.MessageId,
                            levelExpEvent.ChannelId, after);
                    }
                }
            }
        }

        private Task GiveRolesBackAsync(SocketGuildUser user) => _roleManager.GiveRolesBackAsync(user);

        // Event handlers for exp
        private Task ServerMessageExpAsync(SocketMessage message)
        {
            var _ = Task.Run(async () =>
            {
                if (!ValidateUser(message)) return;
                var channel = message.Channel as ITextChannel;
                var user = message.Author as SocketGuildUser;
                if (!_data.ServerCooldown(user)) return;

                using (var db = new DbService())
                {
                    var userdata = await db.GetOrCreateUserData(user);

                    userdata.LastMessage = DateTime.UtcNow;
                    if (!userdata.FirstMessage.HasValue || 
                        userdata.FirstMessage.Value - TimeSpan.FromDays(999) > userdata.FirstMessage.Value) 
                        userdata.FirstMessage = DateTime.UtcNow;

                    await _handler.AddExp(db, user, _calc.GetMessageExp(_data.IsReducedExp(channel)),
                        _calc.GetMessageCredit());
                }
            });
            return Task.CompletedTask;
        }

        private Task GlobalMessageExpAsync(SocketMessage message)
        {
            var _ = Task.Run(async () =>
            {
                if (!ValidateUser(message)) return;
                var channel = message.Channel as ITextChannel;
                var user = message.Author as SocketGuildUser;

                if (!_data.GlobalCooldown(user)) return;

                await _handler.AddGlobalExp(user, _calc.GetMessageExp(_data.IsReducedExp(channel)),
                    _calc.GetMessageCredit());
            });
            return Task.CompletedTask;
        }

        private Task VoiceExpAsync(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            var _ = Task.Run(async () =>
            {
                if (!(user is SocketGuildUser guser)) return;
                if (user.IsBot) return;
                using (var db = new DbService())
                {
                    try
                    {
                        var userdata = await db.GetOrCreateUserData(guser);
                        if (newState.VoiceChannel != null && oldState.VoiceChannel == null)
                        {
                            userdata.VoiceExpTime = DateTime.UtcNow;
                            await db.SaveChangesAsync();
                            return;
                        }
                        if (oldState.VoiceChannel == null || newState.VoiceChannel != null) return;
                        
                        userdata.StatVoiceTime = userdata.StatVoiceTime + (DateTime.UtcNow - userdata.VoiceExpTime);
                        userdata.Sessions = userdata.Sessions + 1;
                        await _handler.AddExp(db, guser, (int) _calc.GetVoiceExp(userdata.VoiceExpTime),
                            (int) _calc.GetVoiceCredit(userdata.VoiceExpTime));
                        await _achievement.AtOnce(guser, DateTime.UtcNow - userdata.VoiceExpTime);
                        await _achievement.TotalTime(guser, DateTime.UtcNow - userdata.VoiceExpTime);
                    }
                    catch (Exception e)
                    {
                        _log.LogAction(LogLevel.Error, e.ToString(), "Level Voice");
                    }
                }
            });
            return Task.CompletedTask;
        }

        private static bool ValidateUser(SocketMessage message)
        {
            if (message.Author.IsBot) return false;
            if (!(message is SocketUserMessage msg)) return false;
            if (msg.Source != MessageSource.User) return false;
            if (!(msg.Channel is ITextChannel)) return false;
            return msg.Author is SocketGuildUser;
        }
    }
}