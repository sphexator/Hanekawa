using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Extensions;
using Hanekawa.Services.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Modules.Administration
{
    [Summary("Event modules for managing events, add participants and award them all at once")]
    [RequireContext(ContextType.Guild)]
    public class EventPayout : InteractiveBase
    {
        [Command("reward Add", RunMode = RunMode.Async)]
        [Alias("eadd", "eventadd")]
        [Summary("Add event participants to the event payout queue (handled by server admins)")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task AddEventParticipantsAsync(IGuildUser user, int amount = 100)
        {
            using (var db = new DbService())
            {
                if (amount < 0) amount = 0;
                var userdata = await db.GetOrCreateEventParticipant(Context.User as SocketGuildUser);
                userdata.Amount = userdata.Amount + amount;
                await db.SaveChangesAsync();
                await ReplyAsync(null, false,
                    new EmbedBuilder()
                        .Reply($"Added {user.Mention} to the payout queue with an addition of {amount} special credit",
                            Color.Green.RawValue).Build());
            }
        }

        [Command("reward Remove", RunMode = RunMode.Async)]
        [Alias("eremove", "eventremove")]
        [Summary("Remove users from the event payout queue")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task RemoveEventParticipantAsync(IGuildUser user)
        {
            using (var db = new DbService())
            {
                var userdata =
                    await db.EventPayouts.FirstOrDefaultAsync(x =>
                        x.GuildId == Context.Guild.Id && x.UserId == user.Id);
                if (userdata == null)
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Couldn't find {user.Mention} in the event payout queue!",
                            Color.Red.RawValue).Build());
                    return;
                }

                db.EventPayouts.Remove(userdata);
                await db.SaveChangesAsync();
                await ReplyAsync(null, false,
                    new EmbedBuilder()
                        .Reply(
                            $"Removed {user.Mention} with an amount of {userdata.Amount} from the event payout queue.",
                            Color.Green.RawValue).Build());
            }
        }

        [Command("reward Adjust", RunMode = RunMode.Async)]
        [Alias("eadjust", "ea", "eventadjust")]
        [Summary("Adjust the reward for a user in the event payout queue")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task AdjustEventParticipant(IGuildUser user, int amount)
        {
            using (var db = new DbService())
            {
                var userdata = await db.GetOrCreateEventParticipant(user as SocketGuildUser);
                userdata.Amount = amount;
                await db.SaveChangesAsync();
                await ReplyAsync(null, false,
                    new EmbedBuilder()
                        .Reply($"Adjusted {user.Mention} special credit to {amount}", Color.Green.RawValue).Build());
            }
        }

        [Command("reward payout", RunMode = RunMode.Async)]
        [Alias("rp", "rewardpayout")]
        [Summary("Payout people that're queued up")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task PayoutEventParticipants()
        {
            using (var db = new DbService())
            {
                var users = await db.EventPayouts.Where(x => x.GuildId == Context.Guild.Id).ToListAsync();
                string payout = null;
                foreach (var x in users)
                {
                    var user = await db.GetOrCreateUserData(x.GuildId, x.UserId);
                    user.CreditSpecial = user.CreditSpecial + (uint)x.Amount;
                    payout += $"{Context.Guild.GetUser(x.UserId).Mention} rewarded {x.Amount}\n";
                }

                await db.SaveChangesAsync();
                db.EventPayouts.RemoveRange(users);
                await db.SaveChangesAsync();

                await ReplyAsync(null, false, new EmbedBuilder().Reply(payout).Build());
            }
        }

        [Command("reward list", RunMode = RunMode.Async)]
        [Alias("el", "elist", "eventlist")]
        [Summary("List all users in the event payout queue")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task ListEventParticipantsAsync()
        {
            using (var db = new DbService())
            {
                var users = await db.EventPayouts.Where(x => x.GuildId == Context.Guild.Id).ToListAsync();
                if (users.Count <= 0)
                {
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"No users in the queue.", Color.Red.RawValue).Build());
                    return;
                }

                string content = null;
                foreach (var x in users) content += $"{Context.Guild.GetUser(x.UserId).Mention} - {x.Amount}\n";
                await ReplyAsync(null, false, new EmbedBuilder().Reply(content).Build());
            }
        }
    }
}