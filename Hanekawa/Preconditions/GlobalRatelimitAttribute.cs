using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Hanekawa.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public class GlobalRatelimitAttribute : PreconditionAttribute
    {
        private readonly uint _invokeLimit;
        private readonly TimeSpan _invokeLimitPeriod;
        private readonly Dictionary<ulong, CommandTimeout> _invokeTracker = new Dictionary<ulong, CommandTimeout>();
        private readonly bool _noLimitForAdmins;
        private readonly bool _noLimitInDMs;

        /// <summary> Sets how often a user is allowed to use this command. </summary>
        /// <param name="times">The number of times a user may use the command within a certain period.</param>
        /// <param name="period">The amount of time since first invoke a user has until the limit is lifted.</param>
        /// <param name="measure">The scale in which the <paramref name="period" /> parameter should be measured.</param>
        /// <param name="noLimitInDMs">Set whether or not there is no limit to the command in DMs. Defaults to false.</param>
        /// <param name="noLimitForAdmins">
        ///     Set whether or not there is no limit to the command for server admins. Defaults to
        ///     false.
        /// </param>
        public GlobalRatelimitAttribute(uint times, double period, Measure measure, bool noLimitInDMs = false,
            bool noLimitForAdmins = false)
        {
            _invokeLimit = times;
            _noLimitInDMs = noLimitInDMs;
            _noLimitForAdmins = noLimitForAdmins;

            //TODO: C# 7 candidate switch expression
            switch (measure)
            {
                case Measure.Days:
                    _invokeLimitPeriod = TimeSpan.FromDays(period);
                    break;
                case Measure.Hours:
                    _invokeLimitPeriod = TimeSpan.FromHours(period);
                    break;
                case Measure.Minutes:
                    _invokeLimitPeriod = TimeSpan.FromMinutes(period);
                    break;
                case Measure.Seconds:
                    _invokeLimitPeriod = TimeSpan.FromSeconds(period);
                    break;
            }
        }

        /// <summary> Sets how often a user is allowed to use this command. </summary>
        /// <param name="times">The number of times a user may use the command within a certain period.</param>
        /// <param name="period">The amount of time since first invoke a user has until the limit is lifted.</param>
        /// <param name="noLimitInDMs">Set whether or not there is no limit to the command in DMs. Defaults to false.</param>
        /// <param name="noLimitForAdmins">
        ///     Set whether or not there is no limit to the command for server admins. Defaults to
        ///     false.
        /// </param>
        public GlobalRatelimitAttribute(uint times, TimeSpan period, bool noLimitInDMs = false,
            bool noLimitForAdmins = false)
        {
            _invokeLimit = times;
            _noLimitInDMs = noLimitInDMs;
            _noLimitForAdmins = noLimitForAdmins;
            _invokeLimitPeriod = period;
        }

        /// <inheritdoc />
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
            IServiceProvider services)
        {
            if (_noLimitInDMs && context.Channel is IPrivateChannel)
                return Task.FromResult(PreconditionResult.FromSuccess());

            if (_noLimitForAdmins && context.User is IGuildUser gu && gu.GuildPermissions.Administrator)
                return Task.FromResult(PreconditionResult.FromSuccess());

            var now = DateTime.UtcNow;
            var timeout = _invokeTracker.TryGetValue(context.Guild.Id, out var t)
                          && now - t.FirstInvoke < _invokeLimitPeriod
                ? t
                : new CommandTimeout(now);

            timeout.TimesInvoked++;

            if (timeout.TimesInvoked > _invokeLimit)
                return Task.FromResult(PreconditionResult.FromError("You are currently in Timeout."));
            _invokeTracker[context.Guild.Id] = timeout;
            return Task.FromResult(PreconditionResult.FromSuccess());
        }

        private class CommandTimeout
        {
            public CommandTimeout(DateTime timeStarted)
            {
                FirstInvoke = timeStarted;
            }

            public uint TimesInvoked { get; set; }
            public DateTime FirstInvoke { get; }
        }
    }
}