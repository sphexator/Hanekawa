using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Data.Variables;
using Jibril.Modules.Fleet.Services;
using Jibril.Preconditions;
using Jibril.Services.Common;

namespace Jibril.Modules.Fleet
{
    public class Fleet : InteractiveBase
    {
        [Command("fleetcreate")]
        [Alias("fc")]
        [Summary("Creates a fleet")]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 5, Measure.Seconds)]
        public async Task CreateFleet([Remainder] string name = null)
        {
            var user = Context.User;
            var auser = Context.User as IGuildUser;
            if (name == null) return;
            if (user == null) return;
            var reqRole = Context.Guild.Roles.FirstOrDefault(r => r.Name == ClassNames.BB);
            var rolecheck = auser.RoleIds.Contains(reqRole.Id);
            var userFleetCheck = FleetDb.CheckFleetMemberStatus(user).FirstOrDefault();
            if (rolecheck && userFleetCheck == "o")
            {
                var nameCheck = FleetDb.CheckFleetName(name).FirstOrDefault();
                if (nameCheck == null && nameCheck != "o")
                {
                    FleetNormDb.CreateFleet(user, name);
                    FleetNormDb.AddLeader(user, name);
                    FleetDb.AddFleet(name);
                    FleetDb.UpdateFleetProfile(user, name);
                    await ReplyAsync($"{user.Username} successfully created a fleet called {name}");
                }
                else if (nameCheck != null)
                {
                    var embed = EmbedGenerator.DefaultEmbed($"{name} is already in use.", Colours.FailColour);
                    await ReplyAsync("", false, embed.Build());
                }
            }
            else
            {
                var embed = EmbedGenerator.DefaultEmbed(
                    $"{user.Username} - You need to be of Battleship rank or not in a fleet inorder to create a fleet.",
                    Colours.FailColour);
                await ReplyAndDeleteAsync("", false, embed.Build());
            }
        }

        [Command("fleetadd", RunMode = RunMode.Async)]
        [Alias("fa")]
        [Summary("Adds a member to your fleet")]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 5, Measure.Seconds)]
        public async Task AddFleetMember(SocketGuildUser member)
        {
            Console.Write($"{Context.User} tried to add {member.Username} to a fleet");

            var user = Context.User;
            if (member.IsBot) return;
            var userFleetCheck = FleetDb.CheckFleetMemberStatus(user).FirstOrDefault();
            var memberFleetCheck = FleetDb.CheckFleetMemberStatus(member).FirstOrDefault();

            if (memberFleetCheck != "o") await ReplyAsync($"{member.Username} is already in a fleet.");
            if (userFleetCheck != "o" && memberFleetCheck == "o")
            {
                var rankCheck = FleetNormDb.RankCheck(user, userFleetCheck).FirstOrDefault();
                if (rankCheck == "leader")
                {
                    await ReplyAsync(
                        $"{member.Mention} has been invited to join `{userFleetCheck}` by {user.Username}\n" +
                        $"\n" +
                        $"Accept/Deny invite");
                    var response = await NextMessageAsync(new EnsureFromUserCriterion(member.Id));

                    if (response.Content.Equals("Accept", StringComparison.InvariantCultureIgnoreCase) ||
                        response.Content.Equals("Yes", StringComparison.InvariantCultureIgnoreCase))
                    {
                        FleetNormDb.AddMember(member, userFleetCheck);
                        FleetDb.UpdateFleetProfile(member, userFleetCheck);
                        FleetDb.AddFleetMemberCount(userFleetCheck);
                        await ReplyAsync($"{member.Username} was added to `{userFleetCheck}` by {user.Username}.");
                    }
                }
                else
                {
                    await ReplyAsync($"{user.Username} isn't a leader and cannot invite");
                }
            }
        }

        [Command("remove", RunMode = RunMode.Async)]
        [Alias("kick")]
        [Summary("Removes a user from your fleet")]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 5, Measure.Seconds)]
        public async Task RemoveFleetMember(IUser member)
        {
            var user = Context.User;
            if (member.IsBot) return;
            var ufc = FleetDb.CheckFleetMemberStatus(user).FirstOrDefault();
            var mfc = FleetDb.CheckFleetMemberStatus(member).FirstOrDefault();
            if (mfc == null || ufc == null) return;
            if (mfc.Equals("o") || ufc.Equals("o")) return;
            if (mfc == ufc)
            {
                var rc = FleetNormDb.RankCheck(user, ufc).FirstOrDefault();
                if (rc == null) return;
                if (rc.Equals("leader", StringComparison.InvariantCultureIgnoreCase))
                {
                    FleetDb.UpdateFleetProfile(member, "o");
                    FleetNormDb.RemoveMember(member, mfc);
                    FleetDb.RemoveFleetMemberCount(mfc);
                    var embed = EmbedGenerator.DefaultEmbed($"{user.Mention} has removed {member} from {mfc}",
                        Colours.OKColour);
                    await ReplyAsync("", false, embed.Build());
                }
                else
                {
                    var embed = EmbedGenerator.DefaultEmbed($"You're not a leader, {user.Mention}.",
                        Colours.FailColour);
                    await ReplyAsync("", false, embed.Build());
                }
            }
            else
            {
                var embed = EmbedGenerator.DefaultEmbed("You're not in the same fleet", Colours.FailColour);
                await ReplyAsync("", false, embed.Build());
            }
        }

        [Command("leave", RunMode = RunMode.Async)]
        [Summary("Leaves a fleet you're a part of")]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 5, Measure.Seconds)]
        public async Task LeaveFleet()
        {
            var user = Context.User;
            var fleet = FleetDb.CheckFleetMemberStatus(user).FirstOrDefault();
            if (fleet == null) return;
            if (fleet.Equals("o")) return;
            var confirmEmbed = EmbedGenerator.DefaultEmbed($"You sure you want to leave {fleet}?\n" +
                                                           $"\n" +
                                                           $"Yes/No", Colours.DefaultColour);
            await ReplyAsync("", false, confirmEmbed.Build());
            var response = await NextMessageAsync();
            if (response.Content.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
            {
                FleetDb.UpdateFleetProfile(user, "o");
                FleetNormDb.RemoveMember(user, fleet);
                FleetDb.RemoveFleetMemberCount(fleet);
                var embed = EmbedGenerator.DefaultEmbed($"{user.Mention} successfully left {fleet}.",
                    Colours.OKColour);
                await ReplyAsync("", false, embed.Build());
            }
            else
            {
                var embed = EmbedGenerator.DefaultEmbed("rip", Colours.FailColour);
                await ReplyAsync("", false, embed.Build());
            }
        }
    }
}