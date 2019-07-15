﻿using System.Threading.Tasks;
using Discord;
using Hanekawa.Shared.Command;

namespace Hanekawa.Shared.Interactive.Criteria
{
    public class EnsureSourceChannelCriterion : ICriterion<IMessage>
    {
        public Task<bool> JudgeAsync(HanekawaContext sourceContext, IMessage parameter)
        {
            var ok = sourceContext.Channel.Id == parameter.Channel.Id;
            return Task.FromResult(ok);
        }
    }
}