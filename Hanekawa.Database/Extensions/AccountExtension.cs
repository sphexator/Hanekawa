using System.Threading.Tasks;
using Disqord;
using Hanekawa.Database.Tables.Account;

namespace Hanekawa.Database.Extensions
{
    public static partial class DbExtensions
    {
        public static async Task<Account> GetOrCreateUserData(this DbService context, CachedMember user) =>
            await GetOrCreateServerUser(context, user.Guild.Id.RawValue, user.Id.RawValue).ConfigureAwait(false);

        public static async Task<Account> GetOrCreateUserData(this DbService context, IGuild guild, IUser user) =>
            await GetOrCreateServerUser(context, guild.Id.RawValue, user.Id.RawValue).ConfigureAwait(false);

        public static async Task<Account> GetOrCreateUserData(this DbService context, ulong guild, ulong user) =>
            await GetOrCreateServerUser(context, guild, user).ConfigureAwait(false);

        public static async Task<AccountGlobal> GetOrCreateGlobalUserData(this DbService context, IUser user) =>
            await GetOrCreateGlobalUser(context, user.Id.RawValue).ConfigureAwait(false);

        public static async Task<AccountGlobal> GetOrCreateGlobalUserData(this DbService context, ulong userId) =>
            await GetOrCreateGlobalUser(context, userId).ConfigureAwait(false);

        public static async Task<AccountGlobal> GetOrCreateGlobalUserData(this DbService context, CachedMember user) =>
            await GetOrCreateGlobalUser(context, user.Id.RawValue).ConfigureAwait(false);

        public static async Task<AccountGlobal> GetOrCreateGlobalUserData(this DbService context, CachedUser user) =>
            await GetOrCreateGlobalUser(context, user.Id.RawValue).ConfigureAwait(false);

        private static async Task<Account> GetOrCreateServerUser(DbService context, ulong guild, ulong user)
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
                if (toReturn != null) return toReturn;
                return data;
            }
            catch
            {
                return data;
            }
        }

        private static async Task<AccountGlobal> GetOrCreateGlobalUser(this DbService context, ulong userId)
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
                if (toReturn != null) return toReturn;
                return data;
            }
            catch
            {
                return data;
            }
        }
    }
}
