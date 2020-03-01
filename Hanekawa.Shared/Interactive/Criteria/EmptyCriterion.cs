﻿using System.Threading.Tasks;
using Hanekawa.Shared.Command;

namespace Hanekawa.Shared.Interactive.Criteria
{
    public class EmptyCriterion<T> : ICriterion<T>
    {
        public Task<bool> JudgeAsync(HanekawaContext sourceContext, T parameter) 
            => Task.FromResult(true);
    }
}