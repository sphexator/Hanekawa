using System;
using Discord;
using System.Threading.Tasks;
using Jibril.Modules.Game.Services;

namespace Jibril.Extensions
{
    public static class DbExtension
    {
        public static async Task<Exp> GetOrCreateUserData(this hanekawaContext context, IUser user)
        {
            var userdata = await context.Exp.FindAsync(user.Id.ToString());
            if (userdata != null) return userdata;
            var data = new Exp
            {
                UserId = user.Id.ToString(),
                Tokens = 0,
                EventTokens = 0,
                Level = 1,
                Xp = 0,
                TotalXp = 0,
                Toxicityavg = 0,
                Toxicitymsgcount = 0,
                Toxicityvalue = 0,
                Rep = 0
            };
            await context.Exp.AddAsync(data);
            return await context.Exp.FindAsync(user.Id.ToString());
        }

        public static async Task<Modlog> GetOrCreateCaseId(this hanekawaContext context, IUser user, DateTime time, int? id = null)
        {
            if (id != null)
            {
                var response = await context.Modlog.FindAsync(id);
                if (response != null) return response;
            }
            var data = new Modlog()
            {
                UserId = user.Id.ToString(),
                Date = time.ToString()
            };
            await context.Modlog.AddAsync(data);
            return (await context.Modlog.FindAsync(time.ToString()));
        }

        public static async Task<Inventory> GetOrCreateInventory(this hanekawaContext context, IUser user)
        {
            var inventory = await context.Inventory.FindAsync(user.Id);
            if (inventory != null) return inventory;
            var data = new Inventory
            {
                Customrole = 0,
                DamageBoost = 0,
                Gift = 0,
                RepairKit = 0,
                Shield = 0,
                UserId = user.Id.ToString()
            };
            await context.Inventory.AddAsync(data);
            return await context.Inventory.FindAsync(user.Id.ToString());
        }

        public static async Task<Shipgame> GetOrCreateShipGame(this hanekawaContext context, IUser user)
        {
            var shipData = await context.Shipgame.FindAsync(user.Id.ToString());
            if (shipData != null) return shipData;
            var userdata = await context.Exp.FindAsync(user.Id.ToString());
            var data = new Shipgame()
            {
                Combatstatus = 0,
                Damagetaken = 0,
                EnemyDamageTaken = 0,
                Enemyhealth = 0,
                Enemyid = 0,
                Health = BaseStats.HealthPoint(userdata.Level, userdata.ShipClass),
                KillAmount = 0,
                UserId = user.Id.ToString()
            };
            await context.Shipgame.AddAsync(data);
            return await context.Shipgame.FindAsync(user.Id.ToString());
        }
    }
}
