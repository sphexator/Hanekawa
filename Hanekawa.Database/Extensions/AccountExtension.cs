using System.Threading.Tasks;
using Disqord;
using Hanekawa.Database.Tables.Account;

namespace Hanekawa.Database.Extensions
{
    public static partial class DbExtensions
    {
        public static async ValueTask<Account> GetOrCreateUserData(this DbService context, IMember user) =>
            await GetOrCreateUserData(context, user.GuildId, user.Id).ConfigureAwait(false);

        public static async ValueTask<Account> GetOrCreateUserData(this DbService context, IGuild guild, IUser user) =>
            await GetOrCreateUserData(context, guild.Id, user.Id).ConfigureAwait(false);
        
        public static async ValueTask<Account> GetOrCreateUserData(this DbService context, Snowflake guild, Snowflake user)
        {
            var userdata = await context.Accounts.FindAsync(guild, user).ConfigureAwait(false);
            if (userdata != null) return userdata;

            var data = new Account().DefaultAccount(guild, user);
            try
            {
                await context.Accounts.AddAsync(data);
                await context.SaveChangesAsync();
                await Task.Delay(20);
                var toReturn = await context.Accounts.FindAsync(user, guild);
                return toReturn ?? data;
            }
            catch
            {
                return data;
            }
        }
        
        public static async ValueTask<AccountGlobal> GetOrCreateGlobalUserDataAsync(this DbService context, IUser user) =>
            await GetOrCreateGlobalUserDataAsync(context, user.Id).ConfigureAwait(false);

        public static async ValueTask<AccountGlobal> GetOrCreateGlobalUserDataAsync(this DbService context, Snowflake userId)
        {
            var userdata = await context.AccountGlobals.FindAsync(userId).ConfigureAwait(false);
            if (userdata != null) return userdata;

            var data = new AccountGlobal().DefaultAccountGlobal(userId);
            try
            {
                await context.AccountGlobals.AddAsync(data);
                await context.SaveChangesAsync();
                await Task.Delay(20);
                var toReturn = await context.AccountGlobals.FindAsync(userId);
                return toReturn ?? data;
            }
            catch
            {
                return data;
            }
        }
    }
}
