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
using Hanekawa.Services.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Modules.Administration
{
    [RequireContext(ContextType.Guild)]
    public class Roles : InteractiveBase
    {
        private readonly LogService _log;
        public Roles(LogService log) => _log = log;

        [Command("Assign")]
        [Alias("iam", "give")]
        [Summary("Assigns a role that's setup as self-assignable")]
        [RequiredChannel]
        public async Task AssignSelfRoleAsync([Remainder] IRole role)
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

                var gUser = (SocketGuildUser) Context.User;
                if (dbRole.Exclusive)
                {
                    var roles = await db.SelfAssignAbleRoles.Where(x => x.GuildId == Context.Guild.Id && x.Exclusive)
                        .ToListAsync();
                    foreach (var x in roles)
                    {
                        var exclusiveRole = Context.Guild.GetRole(x.RoleId);
                        if (gUser.Roles.Contains(exclusiveRole)) await gUser.TryRemoveRoleAsync(exclusiveRole);
                    }

                    await gUser.TryAddRoleAsync(role);
                }
                else
                {
                    await gUser.TryAddRoleAsync(role);
                }

                await ReplyAndDeleteAsync(null, false,
                    new EmbedBuilder().CreateDefault($"Added {role.Name} to {Context.User.Mention}",
                            Color.Green.RawValue)
                        .Build(),
                    TimeSpan.FromSeconds(10));
            }
        }

        [Command("Remove")]
        [Alias("iamn")]
        [Summary("Removes a role that's setup as self-assignable")]
        [RequiredChannel]
        public async Task RemoveSelfRoleAsync([Remainder] IRole role)
        {
            if (!((IGuildUser) Context.User).RoleIds.Contains(role.Id)) return;
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

                await ((IGuildUser) Context.User).TryRemoveRoleAsync(role);
                await ReplyAndDeleteAsync(null, false,
                    new EmbedBuilder()
                        .CreateDefault($"Removed {role.Name} from {Context.User.Mention}", Color.Green.RawValue)
                        .Build(), TimeSpan.FromSeconds(10));
            }
        }

        [Command("role list")]
        [Alias("rl")]
        [Summary("Displays list of self-assignable roles")]
        [RequiredChannel]
        public async Task ListSelfAssignAbleRolesAsync()
        {
            using (var db = new DbService())
            {
                var list = await db.SelfAssignAbleRoles.Where(x => x.GuildId == Context.Guild.Id).ToListAsync();
                if (list.Count == 0)
                {
                    await Context.ReplyAsync("No self-assignable roles added");
                    return;
                }

                string roles = null;
                if (list.Count > 0)
                    foreach (var x in list)
                        try
                        {
                            var role = Context.Guild.GetRole(x.RoleId) ??
                                       Context.Guild.Roles.FirstOrDefault(z => z.Id == x.RoleId);
                            if (role != null) roles += $"**{role.Name}**, ";
                        }
                        catch (Exception e)
                        {
                            _log.LogAction(LogLevel.Error, e.ToString(), "Role module");
                        }
                else roles += "No self-assignable roles added";

                await Context.ReplyAsync(roles);
            }
        }

        [Command("exclusive role add")]
        [Alias("era")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("adds a role to the list of self-assignable roles")]
        public async Task ExclusiveRole([Remainder] IRole role) =>
            await AddSelfAssignAbleRoleAsync(Context, role, true);

        [Command("role add")]
        [Alias("ra")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("adds a role to the list of self-assignable roles")]
        public async Task NonExclusiveRole([Remainder] IRole role) =>
            await AddSelfAssignAbleRoleAsync(Context, role, false);

        [Command("role remove")]
        [Alias("rr")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("Removes a role from the list of self-assignable roles")]
        public async Task RemoveSelfAssignAbleRoleAsync([Remainder] IRole role)
        {
            if ((Context.User as SocketGuildUser).HierarchyCheck(role))
            {
                await Context.ReplyAsync("Can't remove a role that's higher then your highest role.",
                    Color.Red.RawValue);
                return;
            }

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

        private async Task AddSelfAssignAbleRoleAsync(SocketCommandContext context, IRole role, bool exclusive)
        {
            if ((context.User as SocketGuildUser).HierarchyCheck(role))
            {
                await context.ReplyAsync("Can't add a role that's higher then your highest role.", Color.Red.RawValue);
                return;
            }

            using (var db = new DbService())
            {
                var roleCheck = await db.SelfAssignAbleRoles.FindAsync(context.Guild.Id, role.Id);
                if (roleCheck != null)
                {
                    if (exclusive && !roleCheck.Exclusive)
                    {
                        roleCheck.Exclusive = true;
                        await db.SaveChangesAsync();
                        await context.ReplyAsync($"Changed {role.Name} to a exclusive self-assignable role!",
                            Color.Green.RawValue);
                    }
                    else if (!exclusive && roleCheck.Exclusive)
                    {
                        roleCheck.Exclusive = false;
                        await db.SaveChangesAsync();
                        await context.ReplyAsync($"Changed {role.Name} to a non-exclusive self-assignable role!",
                            Color.Green.RawValue);
                    }
                    else
                    {
                        await context.ReplyAsync($"{role.Name} is already added as self-assignable",
                            Color.Red.RawValue);
                    }

                    return;
                }

                var data = new SelfAssignAbleRole
                {
                    GuildId = context.Guild.Id,
                    RoleId = role.Id,
                    Exclusive = exclusive
                };
                await db.SelfAssignAbleRoles.AddAsync(data);
                await db.SaveChangesAsync();
                await context.ReplyAsync($"Added {role.Name} as a self-assignable role!", Color.Green.RawValue);
            }
        }
    }
}