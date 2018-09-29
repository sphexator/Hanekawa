﻿using Discord.Commands;
using Hanekawa.Addons.Database;
using System;
using System.Threading.Tasks;

namespace Hanekawa.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class WhiteListedEventOrg : RequireContextAttribute
    {
        public WhiteListedEventOrg() : base(ContextType.Guild)
        {
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context,
            CommandInfo command, IServiceProvider services)
        {
            using (var db = new DbService())
            {
                var check = await db.WhitelistEvents.FindAsync(context.Guild.Id, context.User.Id);
                return check == null
                    ? PreconditionResult.FromError("Not whitelisted")
                    : PreconditionResult.FromSuccess();
            }
        }
    }
}