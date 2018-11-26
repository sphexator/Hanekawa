using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Tables.GuildConfig;
using Hanekawa.Extensions;
using Hanekawa.Preconditions;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Modules.Administration
{
    public class Roles : InteractiveBase
    {
        [Command("Assign")]
        [Alias("iam", "give")]
        [Summary("Assigns a role that's setup as self-assignable")]
        [RequiredChannel]
        [RequireContext(ContextType.Guild)]
        public async Task AssignSelfRoleAsync(IRole role)
        {
            using (var db = new DbService())
            {
                await Context.Message.DeleteAsync();
                var dbRole = await db.SelfAssignAbleRoles.FindAsync(role.Guild.Id, role.Id);
                if (dbRole == null) return;
                await (Context.User as IGuildUser).AddRoleAsync(role);
                await ReplyAndDeleteAsync(null, false, new EmbedBuilder().Reply($"Added {role.Name} to {Context.User.Mention}", Color.Green.RawValue).Build(), TimeSpan.FromSeconds(10));
            }
        }

        [Command("Remove")]
        [Alias("iamn")]
        [Summary("Removes a role that's setup as self-assignable")]
        [RequiredChannel]
        [RequireContext(ContextType.Guild)]
        public async Task RemoveSelfRoleAsync(IRole role)
        {
            using (var db = new DbService())
            {
                //TODO: Make more complex by checking if role is exclusive or not
                await Context.Message.DeleteAsync();
                var dbRole = await db.SelfAssignAbleRoles.FindAsync(role.Guild.Id, role.Id);
                if (dbRole == null) return;
                await (Context.User as IGuildUser).RemoveRoleAsync(role);
                await ReplyAndDeleteAsync(null, false, new EmbedBuilder().Reply($"Removed {role.Name} from {Context.User.Mention}", Color.Green.RawValue).Build(), TimeSpan.FromSeconds(10));
            }
        }

        [Command("role list")]
        [Alias("rl")]
        [Summary("Displays list of self-assignable roles")]
        [RequiredChannel]
        [RequireContext(ContextType.Guild)]
        public async Task ListSelfAssignAbleRolesAsync(IRole role)
        {
            using (var db = new DbService())
            {
                var list = await db.SelfAssignAbleRoles.Where(x => x.GuildId == Context.Guild.Id).ToListAsync();
                string roles = null;
                if (list.Count > 0)
                {
                    foreach (var x in list)
                    {
                        roles += $"{Context.Guild.GetRole(x.RoleId).Name ?? "Role not found"}, ";
                    }
                }
                else roles += "No self-assignable roles added";
                await ReplyAsync(null, false, new EmbedBuilder().Reply(roles).Build());
            }
        }

        [Command("role add")]
        [Alias("ra")]
        [Summary("adds a role to the list of self-assignable roles")]
        [RequiredChannel]
        [RequireContext(ContextType.Guild)]
        public async Task AddSelfAssignAbleRoleAsync(IRole role, bool exclusive = true)
        {
            using (var db = new DbService())
            {
                var roleCheck = await db.SelfAssignAbleRoles.FindAsync(Context.Guild.Id, role.Id);
                if (roleCheck != null)
                {
                    await ReplyAsync(null, false, new EmbedBuilder().Reply($"{role.Name} is already added as self-assignable", Color.Red.RawValue).Build());
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
                await ReplyAsync(null, false, new EmbedBuilder().Reply($"Added {role.Name} as a self-assignable role!", Color.Green.RawValue).Build());
            }
        }

        [Command("role remove")]
        [Alias("rr")]
        [Summary("Removes a role from the list of self-assignableroles")]
        [RequiredChannel]
        [RequireContext(ContextType.Guild)]
        public async Task RemoveSelfAssignAbleRoleAsync(IRole role)
        {
            using (var db = new DbService())
            {
                var roleCheck = await db.SelfAssignAbleRoles.FirstOrDefaultAsync(x => x.GuildId == Context.Guild.Id && x.RoleId == role.Id);
                if (roleCheck == null)
                {
                    await ReplyAsync(null, false, new EmbedBuilder().Reply($"There is no self-assignable role by the name {role.Name}", Color.Red.RawValue).Build());
                    return;
                }
                db.SelfAssignAbleRoles.Remove(roleCheck);
                await db.SaveChangesAsync();
                await ReplyAsync(null, false, new EmbedBuilder().Reply($"Removed {role.Name} as a self-assignable role!", Color.Green.RawValue).Build());
            }
        }
    }
}