﻿using System;
using System.Threading.Tasks;
using Disqord;
using Qmmands;

namespace Hanekawa.Shared.Command
{
    public sealed class BotOwnerOnlyAttribute : HanekawaAttribute
    {
        public BotOwnerOnlyAttribute() { }

        public override async ValueTask<CheckResult> CheckAsync(HanekawaContext context, IServiceProvider provider)
        {
            switch (context.Bot.TokenType)
            {
                case TokenType.Bot:
                {
                    return (await context.Bot.CurrentApplication.GetAsync().ConfigureAwait(false)).Owner.Id.RawValue == context.User.Id.RawValue
                        ? CheckResult.Successful
                        : CheckResult.Unsuccessful("This can only be executed by the bot's owner.");
                }

                case TokenType.Bearer:
                case TokenType.User:
                {
                    return context.Bot.CurrentUser.Id.RawValue == context.User.Id.RawValue
                        ? CheckResult.Successful
                        : CheckResult.Unsuccessful("This can only be executed by the currently logged in user.");
                }

                default:
                    throw new InvalidOperationException("Invalid token type.");
            }
        }
    }
}
