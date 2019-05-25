using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Core.Interactive.Criteria;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Database.Tables.Club;
using Hanekawa.Extensions.Embed;
using Microsoft.EntityFrameworkCore;
using Qmmands;

namespace Hanekawa.Bot.Modules.Club
{
    public partial class Club
    {
        [Name("Create")]
        [Command("club create")]
        [Description("Creates a club")]
        [Remarks("h.club create Fan service club")]
        [RequiredChannel]
        public async Task CreateClub([Remainder] string name) => await _admin.CreateAsync(Context, name);

        [Name("Club add")]
        [Command("club add")]
        [Description("Adds a member to your club")]
        [Remarks("h.club add @bob#0000")]
        [RequiredChannel]
        public async Task AddClubMemberAsync(SocketGuildUser user)
        {
            if (user == Context.User) return;
            using (var db = new DbService())
            {
                var clubUser =
                    await db.ClubPlayers.FirstOrDefaultAsync(x =>
                        x.GuildId == Context.Guild.Id && x.UserId == Context.User.Id && x.Rank <= 2);
                if (clubUser == null) return;
                if (clubUser.Rank > 2)
                {
                    await Context.ReplyAsync("You're not high enough rank to use that command!", Color.Red.RawValue);
                    return;
                }
                var check = await db.ClubPlayers.FirstOrDefaultAsync(x =>
                    x.UserId == user.Id && x.GuildId == user.GuildId && x.ClubId == clubUser.Id);
                if (check != null)
                {
                    await Context.ReplyAsync($"{user.Mention} is already a member of your club.");
                    return;
                }
                var clubData = await db.GetClubAsync(Context.User, clubUser.ClubId);
                await Context.ReplyAsync(
                    $"{user.Mention}, {Context.User.Mention} has invited you to {clubData.Name}, do you accept? (y/n)");
                var status = true;
                while (status)
                    try
                    {
                        var response = await NextMessageAsync(new EnsureFromUserCriterion(user.Id),
                            TimeSpan.FromSeconds(30));

                        if (response.Content.ToLower() == "y") status = false;
                        if (response.Content.ToLower() == "n") return;
                    }
                    catch
                    {
                        await Context.ReplyAsync("Invite expired.", Color.Red.RawValue);
                        return;
                    }
                await Context.Channel.TriggerTypingAsync();
                await _club.AddUserAsync(user, clubData.Id, db);
                await Context.ReplyAsync($"Added {user.Mention} to {clubData.Name}", Color.Green.RawValue);
            }
        }

        [Name("Club remove")]
        [Command("club remove", "club kick")]
        [Description("Removes a user from your club")]
        [Remarks("h.club remove @bob#0000")]
        [RequiredChannel]
        public async Task RemoveClubMemberAsync(SocketGuildUser user)
        {
            if (user == Context.User) return;
            using (var db = new DbService())
            {
                var club = await db.IsClubLeader(Context.Guild.Id, Context.User.Id);
                if (club == null) return;
                await Context.Channel.TriggerTypingAsync();
                var response = await _club.RemoveUserAsync(user, club.Id, db);
                if (response == null)
                {
                    await Context.ReplyAsync($"Couldn't remove {user.Mention}", Color.Red.RawValue);
                    return;
                }

                await Context.ReplyAsync($"Removed {user.Mention} from {club.Name}", Color.Green.RawValue);
            }
        }

        [Name("Club leave")]
        [Command("club leave")]
        [Description("Leaves a club you're part of")]
        [Remarks("h.club leave")]
        public async Task LeaveClubAsync()
        {
            using (var db = new DbService())
            {
                var clubs = await db.ClubPlayers
                    .Where(x => x.GuildId == Context.Guild.Id && x.UserId == Context.User.Id).ToListAsync();
                ClubUser club = null;
                switch (clubs.Count)
                {
                    case 0:
                        await Context.ReplyAsync("You're currently not in a club.", Color.Red.RawValue);
                        break;
                    case 1:
                        club = clubs.FirstOrDefault();
                        var clubInfo = await _management.RemoveUserAsync(db, Context.User, Context.Guild, club);
                        if (clubInfo != null)
                        {
                            await Context.ReplyAsync(
                                $"Successfully left {clubInfo.Name}!");
                        }
                        break;
                    default:
                    {
                        string content = null;
                        if (clubs.Count != 0)
                            foreach (var x in clubs)
                            {
                                content += $"{x.ClubId} - {(await db.GetClubAsync(x.ClubId, Context.Guild)).Name}\n";
                            }
                        await Context.ReplyAsync(new EmbedBuilder().CreateDefault(content, Context.Guild.Id)
                            .WithTitle("Reply with the ID of club you wish to leave")
                            .WithFooter("Exit to cancel"));
                        var status = true;
                        var retries = 0;
                        while (status)
                            try
                            {
                                retries++;
                                var response = await NextMessageAsync(true, true, TimeSpan.FromSeconds(30));
                                if (response == null)
                                {
                                    if (retries >= 3) status = false;
                                    continue;
                                }

                                if (response.Content.ToLower() == "exit") return;
                                if (!int.TryParse(response.Content, out var result)) continue;
                                club = clubs.FirstOrDefault(x => x.ClubId == result);
                                if (club == null) continue;
                                await _management.RemoveUserAsync(db, Context.User, Context.Guild, club);
                                await Context.ReplyAsync(
                                    $"Successfully left {(await db.GetClubAsync(club.ClubId, Context.Guild)).Name}",
                                    Color.Green.RawValue);
                                status = false;
                            }
                            catch (Exception e)
                            {
                                _log.LogAction(LogLevel.Error, e.ToString(), "Club user leave");
                            }

                        break;
                    }
                }
            }
        }

        [Name("Club promote")]
        [Command("club promote")]
        [Description("Promotes someone to a higher rank")]
        [Remarks("h.club promote @bob#0000")]
        [RequiredChannel]
        public async Task ClubPromoteAsync(SocketGuildUser user)
        {
            if (Context.User == user) return;
            using (var db = new DbService())
            {
                var club = await db.IsClubLeader(Context.Guild.Id, Context.User.Id);
                if (club == null)
                {
                    await Context.ReplyAsync("You can't use this command as you're not a leader of any clubs",
                        Color.Red.RawValue);
                    return;
                }
                var clubUser =
                    await db.ClubPlayers.FirstOrDefaultAsync(x =>
                        x.GuildId == user.GuildId && x.UserId == user.Id && x.Rank <= 2);
                if (clubUser != null && clubUser.ClubId != club.Id)
                {
                    await Context.ReplyAsync($"{user.Mention} is already an officer in a club");
                    return;
                }
                var toPromote = await db.ClubPlayers.FirstOrDefaultAsync(x =>
                    x.ClubId == club.Id && x.GuildId == user.GuildId && x.UserId == user.Id);
                if (toPromote == null)
                {
                    await Context.ReplyAsync($"{user.Mention} is not part of {club.Name}",
                        Color.Red.RawValue);
                    return;
                }

                if (toPromote.Rank == 2)
                {
                    await Context.ReplyAsync($"Are you sure you want to transfer ownership of {club.Name} to {user.Mention}? (y/n)");
                    var response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(45));
                    if (response == null || response.Content.ToLower() != "y")
                    {
                        await Context.ReplyAsync("Cancelling...");
                        return;
                    }
                    await _management.PromoteUserAsync(db, user, toPromote, club);
                    await Context.ReplyAsync($"Transferred ownership of {club.Name} to {user.Mention}");
                }
                else
                {
                    await _management.PromoteUserAsync(db, user, toPromote, club);
                    await Context.ReplyAsync($"Promoted {user.Mention} to rank 2.");
                }

            }
        }

        [Name("Club demote")]
        [Command("club demote")]
        [Description("Demotes someone to a lower rank")]
        [Remarks("club demote @bob#0000")]
        [RequiredChannel]
        public async Task ClubDemoteAsync(SocketGuildUser user)
        {
            await _club.DemoteAsync(user, );
        }
    }
}
