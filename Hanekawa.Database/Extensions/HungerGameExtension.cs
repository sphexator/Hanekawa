using System;
using System.Threading.Tasks;
using Disqord;
using Hanekawa.Database.Tables.Account.HungerGame;
using Hanekawa.Shared.Game.HungerGame;

namespace Hanekawa.Database.Extensions
{
    public static class HungerGameExtension
    {
        public static async Task<HungerGameStatus> GetOrCreateHungerGameStatus(this DbService db, CachedGuild guild) =>
            await GetOrCreateHungerGameStatus(db, guild.Id.RawValue);

        public static async Task<HungerGameStatus> GetOrCreateHungerGameStatus(this DbService db, ulong guildId)
        {
            var response = await db.HungerGameStatus.FindAsync(guildId);
            if (response != null) return response;
            var data = new HungerGameStatus
            {
                GuildId = guildId,
                SignUpChannel = null,
                EventChannel = null,
                EmoteMessageFormat = "<:Rooree:761209568365248513>",
                Stage = HungerGameStage.Closed,
                SignUpStart = DateTimeOffset.UtcNow,
                SignUpMessage = null,
                GameId = null,
                
                ExpReward = 0,
                CreditReward = 0,
                SpecialCreditReward = 0,
                RoleReward = null
            };
            await db.HungerGameStatus.AddAsync(data);
            await db.SaveChangesAsync();
            response = await db.HungerGameStatus.FindAsync(guildId);
            return response ?? data;
        }
    }
}