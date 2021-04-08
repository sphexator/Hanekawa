using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Hanekawa.Database.Tables.Account;

namespace Hanekawa.Database.Extensions
{
    public static partial class DbExtensions
    {
        public static async Task<Account> GetOrCreateUserData(this DbService context, IMember user) =>
            await GetOrCreateUserData(context, user.GuildId.RawValue, user.Id.RawValue).ConfigureAwait(false);

        public static async Task<Account> GetOrCreateUserData(this DbService context, IGuild guild, IUser user) =>
            await GetOrCreateUserData(context, guild.Id.RawValue, user.Id.RawValue).ConfigureAwait(false);
        
        public static async Task<Account> GetOrCreateUserData(this DbService context, ulong guild, ulong user)
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
        
        public static async Task<AccountGlobal> GetOrCreateGlobalUserData(this DbService context, IUser user) =>
            await GetOrCreateGlobalUser(context, user.Id.RawValue).ConfigureAwait(false);

        public static async Task<AccountGlobal> GetOrCreateGlobalUserData(this DbService context, Snowflake userId) =>
            await GetOrCreateGlobalUser(context, userId.RawValue).ConfigureAwait(false);

        public static async Task<AccountGlobal> GetOrCreateGlobalUser(this DbService context, ulong userId)
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
