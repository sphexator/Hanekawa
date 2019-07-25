using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Database;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Interactive;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Modules.Administration
{
    [Name("Self Assignable Roles")]
    [RequiredChannel]
    public class SelfAssignAbleRoles : InteractiveBase
    {
        [Name("I am")]
        [Command("iam", "give")]
        [Description("Assigns a role that's setup as self-assignable")]
        [RequiredChannel]
        public async Task AssignSelfRoleAsync([Remainder] SocketRole role)
        {
            using (var db = Context.Provider.GetRequiredService<DbService>())
            {
                await Context.Message.TryDeleteMessageAsync();
                var dbRole = await db.SelfAssignAbleRoles.FindAsync(role.Guild.Id, role.Id);
                if (dbRole == null)
                {
                    await ReplyAndDeleteAsync("Couldn't find a self-assignable role with that name",
                        timeout: TimeSpan.FromSeconds(15));
                    return;
                }

                bool addedRole;
                var gUser = Context.User;
                if (dbRole.Exclusive)
                {
                    var roles = await db.SelfAssignAbleRoles.Where(x => x.GuildId == Context.Guild.Id && x.Exclusive)
                        .ToListAsync();
                    foreach (var x in roles)
                    {
                        var exclusiveRole = Context.Guild.GetRole(x.RoleId);
                        if (gUser.Roles.Contains(exclusiveRole)) await gUser.TryRemoveRoleAsync(exclusiveRole);
                    }

                    addedRole = await gUser.TryAddRoleAsync(role);
                }
                else
                {
                    addedRole = await gUser.TryAddRoleAsync(role);
                }

                if (addedRole)
                    await ReplyAndDeleteAsync(null, false,
                        new EmbedBuilder().CreateDefault($"Added {role.Name} to {Context.User.Mention}",
                                Color.Green.RawValue)
                            .Build(),
                        TimeSpan.FromSeconds(10));
                else
                    await ReplyAndDeleteAsync(null, false,
                        new EmbedBuilder().CreateDefault(
                                $"Couldn't add {role.Name} to {Context.User.Mention}, missing permission or role position?",
                                Color.Red.RawValue)
                            .Build(),
                        TimeSpan.FromSeconds(10));
            }
        }

        [Name("I am not")]
        [Command("iamnot", "iamn")]
        [Description("Removes a role that's setup as self-assignable")]
        [RequiredChannel]
        public async Task RemoveSelfRoleAsync([Remainder] SocketRole role)
        {
            if (!(Context.User is SocketGuildUser user)) return;
            if (user.Roles.FirstOrDefault(x => x.Id == role.Id) == null) return;
            using (var db = Context.Provider.GetRequiredService<DbService>())
            {
                await Context.Message.TryDeleteMessageAsync();
                var dbRole = await db.SelfAssignAbleRoles.FindAsync(role.Guild.Id, role.Id);
                if (dbRole == null)
                {
                    await ReplyAndDeleteAsync("Couldn't find a self-assignable role with that name",
                        timeout: TimeSpan.FromSeconds(15));
                    return;
                }

                if (await user.TryRemoveRoleAsync(role))
                    await ReplyAndDeleteAsync(null, false,
                        new EmbedBuilder()
                            .CreateDefault($"Removed {role.Name} from {Context.User.Mention}", Color.Green.RawValue)
                            .Build(), TimeSpan.FromSeconds(10));
                else
                    await ReplyAndDeleteAsync(null, false,
                        new EmbedBuilder()
                            .CreateDefault(
                                $"Couldn't remove {role.Name} from {Context.User.Mention}, missing permission or role position?",
                                Color.Red.RawValue)
                            .Build(), TimeSpan.FromSeconds(10));
            }
        }

        [Name("Role list")]
        [Command("rolelist", "rl")]
        [Description("Displays list of self-assignable roles")]
        [RequiredChannel]
        public async Task ListSelfAssignAbleRolesAsync()
        {
            using (var db = Context.Provider.GetRequiredService<DbService>())
            {
                var list = await db.SelfAssignAbleRoles.Where(x => x.GuildId == Context.Guild.Id).ToListAsync();
                if (list == null || list.Count == 0)
                {
                    await Context.ReplyAsync("No self-assignable roles added");
                    return;
                }

                var result = new List<string>();
                foreach (var x in list)
                {
                    var role = Context.Guild.GetRole(x.RoleId) ??
                               Context.Guild.Roles.FirstOrDefault(z => z.Id == x.RoleId);
                    if (role != null) result.Add(x.Exclusive ? $"**{role.Name}** (exclusive)" : $"**{role.Name}**");
                }

                await Context.ReplyPaginated(result, Context.Guild,
                    $"Self-assignable roles for {Context.Guild.Name}", null, 10);
            }
        }

        [Name("Exclusive role add")]
        [Command("era")]
        [Description("Adds a role to the list of self-assignable roles")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task ExclusiveRole([Remainder] SocketRole role) =>
            await AddSelfAssignAbleRoleAsync(Context, role, true);

        [Name("Role add")]
        [Command("roleadd", "ra")]
        [Description("Adds a role to the list of self-assignable roles")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task NonExclusiveRole([Remainder] SocketRole role) =>
            await AddSelfAssignAbleRoleAsync(Context, role, false);

        [Name("Role remove")]
        [Command("roleremove", "rr")]
        [Description("Removes a role from the list of self-assignable roles")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task RemoveSelfAssignAbleRoleAsync([Remainder] SocketRole role)
        {
            if (Context.User.HierarchyCheck(role))
            {
                await Context.ReplyAsync("Can't remove a role that's higher then your highest role.",
                    Color.Red);
                return;
            }

            using (var db = Context.Provider.GetRequiredService<DbService>())
            {
                var roleCheck =
                    await db.SelfAssignAbleRoles.FirstOrDefaultAsync(x =>
                        x.GuildId == Context.Guild.Id && x.RoleId == role.Id);
                if (roleCheck == null)
                {
                    await Context.ReplyAsync($"There is no self-assignable role by the name {role.Name}",
                        Color.Red);
                    return;
                }

                db.SelfAssignAbleRoles.Remove(roleCheck);
                await db.SaveChangesAsync();
                await Context.ReplyAsync($"Removed {role.Name} as a self-assignable role!", Color.Green);
            }
        }

        private async Task AddSelfAssignAbleRoleAsync(HanekawaContext context, SocketRole role, bool exclusive)
        {
            if (context.User.HierarchyCheck(role))
            {
                await context.ReplyAsync("Can't add a role that's higher then your highest role.", Color.Red);
                return;
            }

            using (var db = Context.Provider.GetRequiredService<DbService>())
            {
                var roleCheck = await db.SelfAssignAbleRoles.FindAsync(context.Guild.Id, role.Id);
                if (roleCheck != null)
                {
                    if (exclusive && !roleCheck.Exclusive)
                    {
                        roleCheck.Exclusive = true;
                        await db.SaveChangesAsync();
                        await context.ReplyAsync($"Changed {role.Name} to a exclusive self-assignable role!",
                            Color.Green);
                    }
                    else if (!exclusive && roleCheck.Exclusive)
                    {
                        roleCheck.Exclusive = false;
                        await db.SaveChangesAsync();
                        await context.ReplyAsync($"Changed {role.Name} to a non-exclusive self-assignable role!",
                            Color.Green);
                    }
                    else
                    {
                        await context.ReplyAsync($"{role.Name} is already added as self-assignable",
                            Color.Red);
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
                await context.ReplyAsync($"Added {role.Name} as a self-assignable role!", Color.Green);
            }
        }
    }
}