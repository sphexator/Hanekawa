namespace Jibril.Modules.Administration.Services
{
    public class TimedMuteService
    {
        /*
        private readonly List<Timer> _timer;

        public TimedMuteService()
        {
            _timer = new Timer(_ =>
            {

            }),
        }

        public void StartUnmuteTimer(ulong guildId, ulong userId, TimeSpan after)
        {
            //load the unmute timers for this guild
            var userUnmuteTimers = UnmuteTimers.GetOrAdd(guildId, new ConcurrentDictionary<ulong, Timer>());

            //unmute timer to be added
            var toAdd = new Timer(async _ =>
            {
                try
                {
                    var guild = _client.GetGuild(guildId); // load the guild
                    if (guild == null)
                    {
                        RemoveUnmuteTimerFromDb(guildId, userId);
                        return; // if guild can't be found, just remove the timer from db
                    }
                    // unmute the user, this will also remove the timer from the db
                    await UnmuteUser(guild.GetUser(userId)).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    RemoveUnmuteTimerFromDb(guildId, userId); // if unmute errored, just remove unmute from db
                    _log.Warn("Couldn't unmute user {0} in guild {1}", userId, guildId);
                    _log.Warn(ex);
                }
            }, null, after, Timeout.InfiniteTimeSpan);

            //add it, or stop the old one and add this one
            userUnmuteTimers.AddOrUpdate(userId, (key) => toAdd, (key, old) =>
            {
                old.Change(Timeout.Infinite, Timeout.Infinite);
                return toAdd;
            });
        }
        */
    }
}