using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Tables.GuildConfig;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Preconditions;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Modules.Administration
{
    [RequiredChannel]
    [RequireContext(ContextType.Guild)]
    public class Roles : InteractiveBase
    {
        [Command("Assign")]
        [Alias("iam", "give")]
        [Summary("Assigns a role that's setup as self-assignable")]
        public async Task AssignSelfRoleAsync(IRole role)
        {
            using (var db = new DbService())
            {
                await Context.Message.DeleteAsync();
                var dbRole = await db.SelfAssignAbleRoles.FindAsync(role.Guild.Id, role.Id);
                if (dbRole == null)
                {
                    await ReplyAndDeleteAsync("Couldn't find a self-assignable role with that name",
                        timeout: TimeSpan.FromSeconds(15));
                    return;
                }

                var gUser = (SocketGuildUser)Context.User;
                if (dbRole.Exclusive)
                {
                    var roles = await db.SelfAssignAbleRoles.Where(x => x.GuildId == Context.Guild.Id && x.Exclusive)
                        .ToListAsync();
                    foreach (var x in roles)
                    {
                        var exclusiveRole = Context.Guild.GetRole(x.RoleId);
                        if (gUser.Roles.Contains(exclusiveRole)) await gUser.RemoveRoleAsync(exclusiveRole);
                    }

                    await gUser.AddRoleAsync(role);
                }
                else
                {
                    await gUser.AddRoleAsync(role);
                }

                await ReplyAndDeleteAsync(null, false,
                    new EmbedBuilder().CreateDefault($"Added {role.Name} to {Context.User.Mention}", Color.Green.RawValue)
                        .Build(),
                    TimeSpan.FromSeconds(10));
            }
        }

        [Command("Remove")]
        [Alias("iamn")]
        [Summary("Removes a role that's setup as self-assignable")]
        public async Task RemoveSelfRoleAsync(IRole role)
        {
            using (var db = new DbService())
            {
                await Context.Message.DeleteAsync();
                var dbRole = await db.SelfAssignAbleRoles.FindAsync(role.Guild.Id, role.Id);
                if (dbRole == null)
                {
                    await ReplyAndDeleteAsync("Couldn't find a self-assignable role with that name",
                        timeout: TimeSpan.FromSeconds(15));
                    return;
                }

                await (Context.User as IGuildUser).RemoveRoleAsync(role);
                await ReplyAndDeleteAsync(null, false,
                    new EmbedBuilder()
                        .CreateDefault($"Removed {role.Name} from {Context.User.Mention}", Color.Green.RawValue)
                        .Build(), TimeSpan.FromSeconds(10));
            }
        }

        [Command("role list")]
        [Alias("rl")]
        [Summary("Displays list of self-assignable roles")]
        public async Task ListSelfAssignAbleRolesAsync(IRole role)
        {
            using (var db = new DbService())
            {
                var list = await db.SelfAssignAbleRoles.Where(x => x.GuildId == Context.Guild.Id).ToListAsync();
                string roles = null;
                if (list.Count > 0)
                    foreach (var x in list)
                        roles += $"{Context.Guild.GetRole(x.RoleId).Name ?? "*Role deleted*"}, ";
                else roles += "No self-assignable roles added";
                await Context.ReplyAsync(roles);
            }
        }

        [Command("role add")]
        [Alias("ra")]
        [Summary("adds a role to the list of self-assignable roles")]
        public async Task AddSelfAssignAbleRoleAsync(IRole role, bool exclusive = false)
        {
            using (var db = new DbService())
            {
                var roleCheck = await db.SelfAssignAbleRoles.FindAsync(Context.Guild.Id, role.Id);
                if (roleCheck != null)
                {
                    await Context.ReplyAsync($"{role.Name} is already added as self-assignable", Color.Red.RawValue);
                    return;
                }

                var data = new SelfAssignAbleRole
                {
                    GuildId = Context.Guild.Id,
                    RoleId = role.Id,
                    Exclusive = exclusive
                };
                await db.SelfAssignAbleRoles.AddAsync(data);
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Added {role.Name} as a self-assignable role!", Color.Green.RawValue);
            }
        }

        [Command("role remove")]
        [Alias("rr")]
        [Summary("Removes a role from the list of self-assignableroles")]
        public async Task RemoveSelfAssignAbleRoleAsync(IRole role)
        {
            using (var db = new DbService())
            {
                var roleCheck =
                    await db.SelfAssignAbleRoles.FirstOrDefaultAsync(x =>
                        x.GuildId == Context.Guild.Id && x.RoleId == role.Id);
                if (roleCheck == null)
                {
                    await Context.ReplyAsync($"There is no self-assignable role by the name {role.Name}",
                        Color.Red.RawValue);
                    return;
                }

                db.SelfAssignAbleRoles.Remove(roleCheck);
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Removed {role.Name} as a self-assignable role!", Color.Green.RawValue);
            }
        }
    }
}