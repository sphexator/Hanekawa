using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Bot.Preconditions;
using Hanekawa.Database;
using Hanekawa.Database.Tables.Config;
using Hanekawa.Extensions;
using Hanekawa.Extensions.Embed;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Command.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Modules.Administration
{
    [Name("Self Assignable Roles")]
    [RequiredChannel]
    public class SelfAssignAbleRoles : HanekawaCommandModule
    {
        [Name("I am")]
        [Command("iam", "give")]
        [Description("Assigns a role that's setup as self-assignable")]
        [RequiredChannel]
        public async Task AssignSelfRoleAsync([Remainder] CachedRole role)
        {
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            await Context.Message.TryDeleteMessageAsync();
            var dbRole = await db.SelfAssignAbleRoles.FindAsync(role.Guild.Id.RawValue, role.Id.RawValue);
            if (dbRole == null)
            {
                await Context.ReplyAndDeleteAsync("Couldn't find a self-assignable role with that name",
                    timeout: TimeSpan.FromSeconds(15));
                return;
            }

            bool addedRole;
            var gUser = Context.Member;
            if (dbRole.Exclusive)
            {
                var roles = await db.SelfAssignAbleRoles.Where(x => x.GuildId == Context.Guild.Id.RawValue && x.Exclusive)
                    .ToListAsync();
                foreach (var x in roles)
                {
                    var exclusiveRole = Context.Guild.GetRole(x.RoleId);
                    if (gUser.Roles.Values.Contains(exclusiveRole)) await gUser.TryRemoveRoleAsync(exclusiveRole);
                }

                addedRole = await gUser.TryAddRoleAsync(role);
            }
            else
            {
                addedRole = await gUser.TryAddRoleAsync(role);
            }

            if (addedRole)
            {
                await Context.ReplyAndDeleteAsync(null, false,
                    new LocalEmbedBuilder().Create($"Added {role.Name} to {Context.User.Mention}",
                        Color.Green),TimeSpan.FromSeconds(10));
            }
            else
            {
                await Context.ReplyAndDeleteAsync(null, false,
                    new LocalEmbedBuilder().Create(
                        $"Couldn't add {role.Name} to {Context.User.Mention}, missing permission or role position?",
                        Color.Red),TimeSpan.FromSeconds(10));
            }
        }

        [Name("I am not")]
        [Command("iamnot", "iamn")]
        [Description("Removes a role that's setup as self-assignable")]
        [RequiredChannel]
        public async Task RemoveSelfRoleAsync([Remainder] CachedRole role)
        {
            if (Context.Member.Roles.Values.FirstOrDefault(x => x.Id.RawValue == role.Id.RawValue) == null) return;
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            await Context.Message.TryDeleteMessageAsync();
            var dbRole = await db.SelfAssignAbleRoles.FindAsync(role.Guild.Id.RawValue, role.Id.RawValue);
            if (dbRole == null)
            {
                await Context.ReplyAndDeleteAsync("Couldn't find a self-assignable role with that name",
                    timeout: TimeSpan.FromSeconds(15));
                return;
            }

            if (await Context.Member.TryRemoveRoleAsync(role))
            {
                await Context.ReplyAndDeleteAsync(null, false,
                    new LocalEmbedBuilder()
                        .Create($"Removed {role.Name} from {Context.User.Mention}", Color.Green), TimeSpan.FromSeconds(10));
            }
            else
            {
                await Context.ReplyAndDeleteAsync(null, false,
                    new LocalEmbedBuilder()
                        .Create(
                            $"Couldn't remove {role.Name} from {Context.User.Mention}, missing permission or role position?",
                            Color.Red), TimeSpan.FromSeconds(10));
            }
        }

        [Name("Role list")]
        [Command("rolelist", "rl")]
        [Description("Displays list of self-assignable roles")]
        [RequiredChannel]
        public async Task ListSelfAssignAbleRolesAsync()
        {
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var list = await db.SelfAssignAbleRoles.Where(x => x.GuildId == Context.Guild.Id.RawValue).ToListAsync();
            if (list == null || list.Count == 0)
            {
                await Context.ReplyAsync("No self-assignable roles added");
                return;
            }

            var result = new List<string>();
            foreach (var x in list)
            {
                var role = Context.Guild.GetRole(x.RoleId) ??
                           Context.Guild.Roles.Values.FirstOrDefault(z => z.Id.RawValue == x.RoleId);
                if (role != null) result.Add(x.Exclusive ? $"**{role.Name}** (exclusive)" : $"**{role.Name}**");
            }

            await Context.PaginatedReply(result, Context.Guild,
                $"Self-assignable roles for {Context.Guild.Name}");
        }

        [Name("Exclusive role add")]
        [Command("era")]
        [Description("Adds a role to the list of self-assignable roles")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task ExclusiveRole([Remainder] CachedRole role) =>
            await AddSelfAssignAbleRoleAsync(Context, role, true);

        [Name("Role add")]
        [Command("roleadd", "ra")]
        [Description("Adds a role to the list of self-assignable roles")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task NonExclusiveRole([Remainder] CachedRole role) =>
            await AddSelfAssignAbleRoleAsync(Context, role, false);

        [Name("Role remove")]
        [Command("roleremove", "rr")]
        [Description("Removes a role from the list of self-assignable roles")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task RemoveSelfAssignAbleRoleAsync([Remainder] CachedRole role)
        {
            if (!Context.Member.HierarchyCheck(role))
            {
                await Context.ReplyAsync("Can't remove a role that's higher then your highest role.",
                    Color.Red);
                return;
            }

            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var roleCheck =
                await db.SelfAssignAbleRoles.FirstOrDefaultAsync(x =>
                    x.GuildId == Context.Guild.Id.RawValue && x.RoleId == role.Id.RawValue);
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

        private async Task AddSelfAssignAbleRoleAsync(DiscordCommandContext context, CachedRole role, bool exclusive)
        {
            if (!context.Member.HierarchyCheck(role))
            {
                await Context.ReplyAsync("Can't add a role that's higher then your highest role.", Color.Red);
                return;
            }

            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var roleCheck = await db.SelfAssignAbleRoles.FindAsync(context.Guild.Id.RawValue, role.Id.RawValue);
            if (roleCheck != null)
            {
                if (exclusive && !roleCheck.Exclusive)
                {
                    roleCheck.Exclusive = true;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync($"Changed {role.Name} to a exclusive self-assignable role!",
                        Color.Green);
                }
                else if (!exclusive && roleCheck.Exclusive)
                {
                    roleCheck.Exclusive = false;
                    await db.SaveChangesAsync();
                    await Context.ReplyAsync($"Changed {role.Name} to a non-exclusive self-assignable role!",
                        Color.Green);
                }
                else
                {
                    await Context.ReplyAsync($"{role.Name} is already added as self-assignable",
                        Color.Red);
                }

                return;
            }

            var data = new SelfAssignAbleRole
            {
                GuildId = context.Guild.Id.RawValue,
                RoleId = role.Id.RawValue,
                Exclusive = exclusive
            };
            await db.SelfAssignAbleRoles.AddAsync(data);
            await db.SaveChangesAsync();
            await Context.ReplyAsync($"Added {role.Name} as a self-assignable role!", Color.Green);
        }
    }
}