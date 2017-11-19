using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Jibril.Data.Variables;
using Jibril.Extensions;
using Jibril.Preconditions;
using Jibril.Services.Common;
/*
namespace Jibril.Modules.Fleet
{
    public class Fleet : InteractiveBase
    {
        [Command("createfleet")]
        [Alias("cf")]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 5, Measure.Seconds)]
        public async Task CreateFleet([Remainder] string name = null)
        {
            var user = Context.User;
            var auser = Context.User as IGuildUser;
            if (name == null) return;
            if (user == null) return;
            var reqRole = Context.Guild.Roles.FirstOrDefault(r => r.Name == ClassNames.BB); var rolecheck = auser.RoleIds.Contains(reqRole.Id);
            var userFleetCheck = DbService.CheckFleetMemberStatus(user).FirstOrDefault();
            if (rolecheck == true && userFleetCheck == "o")
            {
                var nameCheck = DbService.CheckFleetName(name).FirstOrDefault();
                if (nameCheck == null && nameCheck != "o")
                {
                    FleetDB.CreateFleet(user, name);
                    FleetDB.addLeader(user, name);
                    DbService.AddFleet(name);
                    DbService.UpdateFleetProfile(user, name);
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

        [Command("add", RunMode = RunMode.Async)]
        [Summary("Adds a member to your fleet")]
        [RequiredChannel(339383206669320192)]
        [Ratelimit(1, 5, Measure.Seconds)]
        public async Task AddFleetMember(SocketUser member)
        {
            Console.Write($"{Context.User} tried to add {member.Username} to a fleet");

            var user = Context.User;
            if (member == null) return;
            if (member.IsBot == true) return;
            var userFleetCheck = DbService.CheckFleetMemberStatus(user).FirstOrDefault();
            var memberFleetCheck = DbService.CheckFleetMemberStatus(member).FirstOrDefault();
            if (userFleetCheck != "o" && memberFleetCheck == "o")
            {
                var rankCheck = FleetDB.RankCheck(user, userFleetCheck).FirstOrDefault();
                if (rankCheck == "leader")
                {
                    await ReplyAsync(
                        $"{member.Mention} has been invited to join `{userFleetCheck}` by {user.Username}\n" +
                        $"\n" +
                        $"Accept/Deny invite");
                    var response = await NextMessageAsync(new EnsureFromUserCriterion(member.Id));

                    if (response.Content.Equals("Accept", StringComparison.InvariantCultureIgnoreCase) || response.Content.Equals("Yes", StringComparison.InvariantCultureIgnoreCase))
                    {
                        FleetDB.AddMember(member, userFleetCheck);
                        DbService.UpdateFleetProfile(member, userFleetCheck);
                        DbService.AddFleetMember(userFleetCheck);
                        await ReplyAsync($"{member.Username} was added to `{userFleetCheck}` by {user.Username}.");
                    }
                    else
                    {
                        await ReplyAsync($"{user.Username} isn't a leader and cannot invite");
                    }
                }
                else if (userFleetCheck == "o")
                {
                    return;
                }
                else if (memberFleetCheck != "o")
                {
                    await ReplyAsync($"{member.Username} is already in a fleet.");
                }
            }
        }
    }
}
*/