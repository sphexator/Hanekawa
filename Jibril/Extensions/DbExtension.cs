using System;
using Discord;
using System.Threading.Tasks;

namespace Jibril.Extensions
{
    public static class DbExtension
    {
        public static async Task<Exp> GetOrAddUserData(this hanekawaContext context, IUser user)
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

        public static async Task<int> CreateCaseId(this hanekawaContext context, IUser user, DateTime time)
        {
            var data = new Modlog()
            {
                UserId = user.Id.ToString(),
                Date = time.ToString()
            };
            await context.Modlog.AddAsync(data);
            return (await context.Modlog.FindAsync(time.ToString())).Id;
        }
    }
}
