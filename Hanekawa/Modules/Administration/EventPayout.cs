using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Extensions.Embed;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Hanekawa.Modules.Administration
{
    [Summary("Event modules for managing events, add participants and award them all at once")]
    [RequireContext(ContextType.Guild)]
    public class EventPayout : InteractiveBase
    {
        private readonly DbService _db;

        public EventPayout(DbService db)
        {
            _db = db;
        }

        [Command("reward Add", RunMode = RunMode.Async)]
        [Alias("eadd", "eventadd")]
        [Summary("Add event participants to the event payout queue (handled by server admins)")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task AddEventParticipantsAsync(IGuildUser user, int amount = 100)
        {
            if (amount < 0) amount = 0;
            var userdata = await _db.GetOrCreateEventParticipant(user as SocketGuildUser);
            userdata.Amount = userdata.Amount + amount;
            await _db.SaveChangesAsync();
            await Context.ReplyAsync(
                $"Added {user.Mention} to the payout queue with an addition of {amount} special credit",
                Color.Green.RawValue);
        }

        [Command("reward Remove", RunMode = RunMode.Async)]
        [Alias("eremove", "eventremove")]
        [Summary("Remove users from the event payout queue")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task RemoveEventParticipantAsync(IGuildUser user)
        {
            var userdata =
                await _db.EventPayouts.FirstOrDefaultAsync(x =>
                    x.GuildId == Context.Guild.Id && x.UserId == user.Id);
            if (userdata == null)
            {
                await Context.ReplyAsync($"Couldn't find {user.Mention} in the event payout queue!",
                    Color.Red.RawValue);
                return;
            }

            _db.EventPayouts.Remove(userdata);
            await _db.SaveChangesAsync();
            await Context.ReplyAsync(
                $"Removed {user.Mention} with an amount of {userdata.Amount} from the event payout queue.",
                Color.Green.RawValue);
        }

        [Command("reward Adjust", RunMode = RunMode.Async)]
        [Alias("eadjust", "ea", "eventadjust")]
        [Summary("Adjust the reward for a user in the event payout queue")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task AdjustEventParticipant(IGuildUser user, int amount)
        {
            var userdata = await _db.GetOrCreateEventParticipant(user as SocketGuildUser);
            userdata.Amount = amount;
            await _db.SaveChangesAsync();
            await Context.ReplyAsync($"Adjusted {user.Mention} special credit to {amount}", Color.Green.RawValue);
        }

        [Command("reward payout", RunMode = RunMode.Async)]
        [Alias("rp", "rewardpayout")]
        [Summary("Payout people that're queued up")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task PayoutEventParticipants()
        {
            var users = await _db.EventPayouts.Where(x => x.GuildId == Context.Guild.Id).ToListAsync();
            string payout = null;
            foreach (var x in users)
            {
                var user = await _db.GetOrCreateUserData(x.GuildId, x.UserId);
                user.CreditSpecial = user.CreditSpecial + (uint) x.Amount;
                payout += $"{Context.Guild.GetUser(x.UserId).Mention} rewarded {x.Amount}\n";
            }

            await _db.SaveChangesAsync();
            _db.EventPayouts.RemoveRange(users);
            await _db.SaveChangesAsync();
            await Context.ReplyAsync(payout);
        }

        [Command("reward list", RunMode = RunMode.Async)]
        [Alias("el", "elist", "eventlist")]
        [Summary("List all users in the event payout queue")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task ListEventParticipantsAsync()
        {
            var users = await _db.EventPayouts.Where(x => x.GuildId == Context.Guild.Id).ToListAsync();
            if (users.Count <= 0)
            {
                await Context.ReplyAsync("No users in the queue.", Color.Red.RawValue);
                return;
            }

            string content = null;
            foreach (var x in users) content += $"{Context.Guild.GetUser(x.UserId).Mention} - {x.Amount}\n";
            await Context.ReplyAsync(content);
        }
    }
}