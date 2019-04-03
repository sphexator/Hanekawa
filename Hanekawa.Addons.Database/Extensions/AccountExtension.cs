using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Addons.Database.Tables.Account;

namespace Hanekawa.Addons.Database.Extensions
{
    public static partial class DbExtensions
    {
        public static async Task<Account> GetOrCreateUserData(this DbService context, SocketGuildUser user) =>
            await GetOrCreateServerUser(context, user.Guild.Id, user.Id);

        public static async Task<Account> GetOrCreateUserData(this DbService context, IGuild guild, IUser user) =>
            await GetOrCreateServerUser(context, guild.Id, user.Id);

        public static async Task<Account> GetOrCreateUserData(this DbService context, ulong guild, ulong user) =>
            await GetOrCreateServerUser(context, guild, user);

        public static async Task<Account> GetOrCreateUserData(this DbService context, IGuildUser user) =>
            await GetOrCreateServerUser(context, user.GuildId, user.Id);

        public static async Task<AccountGlobal> GetOrCreateGlobalUserData(this DbService context, IUser user) =>
            await GetOrCreateGlobalUser(context, user.Id);

        public static async Task<AccountGlobal> GetOrCreateGlobalUserData(this DbService context, ulong userId) =>
            await GetOrCreateGlobalUser(context, userId);

        public static async Task<AccountGlobal> GetOrCreateGlobalUserData(this DbService context, IGuildUser user) =>
            await GetOrCreateGlobalUser(context, user.Id);

        public static async Task<AccountGlobal>
            GetOrCreateGlobalUserData(this DbService context, SocketGuildUser user) =>
            await GetOrCreateGlobalUser(context, user.Id);

        public static async Task<AccountGlobal> GetOrCreateGlobalUserData(this DbService context, SocketUser user) =>
            await GetOrCreateGlobalUser(context, user.Id);

        private static async Task<Account> GetOrCreateServerUser(DbService context, ulong guild, ulong user)
        {
            var userdata = await context.Accounts.FindAsync(guild, user);
            if (userdata != null) return userdata;

            var data = new Account().DefaultAccount(guild, user);
            await context.Accounts.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.Accounts.FindAsync(user, guild);
        }

        private static async Task<AccountGlobal> GetOrCreateGlobalUser(this DbService context, ulong userId)
        {
            var userdata = await context.AccountGlobals.FindAsync(userId);
            if (userdata != null) return userdata;

            var data = new AccountGlobal().DefaultAccountGlobal(userId);
            await context.AccountGlobals.AddAsync(data);
            await context.SaveChangesAsync();
            return await context.AccountGlobals.FindAsync(userId);
        }
    }
}
