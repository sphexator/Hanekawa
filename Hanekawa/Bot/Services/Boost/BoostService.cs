using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Events;
using Hanekawa.Bot.Services.Experience;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Shared.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Hanekawa.Bot.Services.Boost
{
    public class BoostService : IRequired, INService
    {
        private readonly Hanekawa _client;
        private readonly ExpService _exp;
        private readonly IServiceProvider _provider;
        public BoostService(Hanekawa client, IServiceProvider provider, ExpService exp)
        {
            _client = client;
            _provider = provider;
            _exp = exp;

            _client.MemberUpdated += BoostCheck;
        }

        private Task BoostCheck(MemberUpdatedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if (!e.OldMember.IsBoosting && e.NewMember.IsBoosting) await StartedBoostingAsync(e.NewMember);
                if (e.OldMember.IsBoosting && !e.NewMember.IsBoosting) await EndedBoostingAsync(e.NewMember);
            });
            return Task.CompletedTask;
        }

        private async Task StartedBoostingAsync(CachedMember user)
        {
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
            var userData = await db.GetOrCreateUserData(user);
            var config = await db.GetOrCreateBoostConfigAsync(user.Guild);
            await _exp.AddExpAsync(user, userData, config.ExpGain, config.CreditGain, db);
            if (config.SpecialCreditGain > 0) userData.CreditSpecial += config.SpecialCreditGain;
            await db.SaveChangesAsync();
        }

        private async Task EndedBoostingAsync(CachedMember user)
        {
            using var scope = _provider.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DbService>();

        }
    }
}