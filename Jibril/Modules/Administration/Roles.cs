using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Extensions;
using Hanekawa.Services;
using Hanekawa.Services.Entities;
using Hanekawa.Services.Entities.Tables;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Modules.Administration {
    public class Roles : InteractiveBase {
        [Command ("Assign")]
        [Alias ("iam", "give")]
        [Summary ("Assigns a role that's setup as self-assignable")]
        [RequireContext (ContextType.Guild)]
        public async Task AssignSelfRoleAsync (IRole role) {
            using (var db = new DbService ()) {
                await Context.Message.DeleteAsync ();
                var dbRole = await db.SelfAssignAbleRoles.FindAsync (role.Guild.Id, role.Id);
                if (dbRole == null) return;
                await (Context.User as IGuildUser).AddRoleAsync (role);
                await ReplyAndDeleteAsync (null, false, new EmbedBuilder ().Reply ($"Added {role.Name} to {Context.User.Mention}", Color.Green.RawValue).Build (), TimeSpan.FromSeconds (10));
            }
        }

        [Command ("Remove")]
        [Alias ("iamn")]
        [Summary ("Removes a role that's setup as self-assignable")]
        public async Task RemoveSelfRoleAsync (IRole role) {
            using (var db = new DbService ()) {
                //TODO: Make more complex by checking if role is exclusive or not
                await Context.Message.DeleteAsync ();
                var dbRole = await db.SelfAssignAbleRoles.FindAsync (role.Guild.Id, role.Id);
                if (dbRole == null) return;
                await (Context.User as IGuildUser).RemoveRoleAsync (role);
                await ReplyAndDeleteAsync (null, false, new EmbedBuilder ().Reply ($"Removed {role.Name} from {Context.User.Mention}", Color.Green.RawValue).Build (), TimeSpan.FromSeconds (10));
            }
        }

        [Command ("role list")]
        [Alias ("rl")]
        [Summary ("Displays list of self-assignable roles")]
        public async Task ListSelfAssignAbleRolesAsync (IRole role) {
            using (var db = new DbService ()) {
                var list = await db.SelfAssignAbleRoles.Where (x => x.GuildId == Context.Guild.Id).ToListAsync ();
                string roles = null;
                if (list.Count > 0) {
                    foreach (var x in list) {
                        roles += $"{Context.Guild.GetRole(x.RoleId).Name ?? "Role not found"}, ";
                    }
                }
                else roles += "No self-assignable roles added";
                await ReplyAsync(null, false, new EmbedBuilder().Reply(roles).Build());
            }
        }

        [Command ("role add")]
        [Alias ("ra")]
        [Summary ("adds a role to the list of self-assignable roles")]
        public async Task AddSelfAssignAbleRoleAsync (IRole role, bool exclusive = true) {
            using (var db = new DbService ()) {
                var roleCheck = await db.SelfAssignAbleRoles.FindAsync(Context.Guild.Id, role.Id);
                if(roleCheck != null){
                    await ReplyAsync(null, false, new EmbedBuilder().Reply($"{role.Name} is already added as self-assignable", Color.Red.RawValue).Build());
                    return;
                }
                var data = new SelfAssignAbleRole{
                    GuildId = Context.Guild.Id,
                    RoleId = role.Id,
                    Exclusive = exclusive
                };
                await db.SelfAssignAbleRoles.AddAsync(data);
                await db.SaveChangesAsync();
                await ReplyAsync(null, false, new EmbedBuilder().Reply($"Added {role.Name} as a self-assignable role!", Color.Green.RawValue).Build());
            }
        }

        [Command ("role remove")]
        [Alias ("rr")]
        [Summary ("Removes a role from the list of self-assignableroles")]
        public async Task RemoveSelfAssignAbleRoleAsync (IRole role) {
                        using (var db = new DbService ()) {
                var roleCheck = await db.SelfAssignAbleRoles.FirstOrDefaultAsync(x => x.GuildId == Context.Guild.Id && x.RoleId == role.Id);
                if(roleCheck == null){
                    await ReplyAsync(null, false, new EmbedBuilder().Reply($"There is no self-assignable role by the name {role.Name}", Color.Red.RawValue).Build());
                    return;
                }
                db.SelfAssignAbleRoles.Remove(roleCheck);
                await db.SaveChangesAsync();
                await ReplyAsync(null, false, new EmbedBuilder().Reply($"Removed {role.Name} as a self-assignable role!", Color.Green.RawValue).Build());
            }
        }

        [Command ("lewd")]
        [Alias ("nsfw")]
        public async Task AssignLewd () {
            var channel = Context.Channel.Id.ToString ();
            if (channel == "339380254097539072" || channel == "339383206669320192")
                try {
                    var user = Context.User as SocketGuildUser;
                    var altUser = Context.User as IGuildUser;
                    var lewdRole = Context.Guild.Roles.First (x => x.Id == 339711429211062273);
                    var roleCheck = altUser.RoleIds.Contains (lewdRole.Id);
                    if (roleCheck == false) {
                        await Context.Message.DeleteAsync ().ConfigureAwait (false);
                        await user.AddRoleAsync (lewdRole).ConfigureAwait (false);
                        var embed = new EmbedBuilder ();
                        embed.WithColor (Color.Purple);
                        embed.Description = "Assigned lewd role";
                        await ReplyAndDeleteAsync ("", false, embed.Build (), TimeSpan.FromSeconds (5))
                            .ConfigureAwait (false);
                    } else if (roleCheck) {
                        await Context.Message.DeleteAsync ().ConfigureAwait (false);
                        await user.RemoveRoleAsync (lewdRole).ConfigureAwait (false);
                        var embed = new EmbedBuilder ();
                        embed.WithColor (Color.Purple);
                        embed.Description = "Removed lewd role";
                        await ReplyAndDeleteAsync ("", false, embed.Build (), TimeSpan.FromSeconds (5))
                            .ConfigureAwait (false);
                    }
                }
            catch {
                await ReplyAndDeleteAsync (":Thinking:", false, null, TimeSpan.FromSeconds (5)).ConfigureAwait (false);
            }
        }

        [Command ("picdump")]
        [Alias ("pic", "pdump", "picd")]
        public async Task AssignPicDump () {
            var channel = Context.Channel.Id.ToString ();
            if (channel == "339380254097539072" || channel == "339383206669320192")
                try {
                    var user = Context.User as SocketGuildUser;
                    var altUser = Context.User as IGuildUser;
                    var picdump = Context.Guild.Roles.FirstOrDefault (r => r.Name == "picdump");
                    var roleCheck = altUser.RoleIds.Contains (picdump.Id);
                    if (roleCheck == false) {
                        await Context.Message.DeleteAsync ().ConfigureAwait (false);
                        await user.AddRoleAsync (picdump).ConfigureAwait (false);
                        var embed = new EmbedBuilder ();
                        embed.WithColor (Color.Purple);
                        embed.Description = "Assigned picdump role";
                        await ReplyAndDeleteAsync ("", false, embed.Build (), TimeSpan.FromSeconds (5))
                            .ConfigureAwait (false);
                    } else if (roleCheck) {
                        await Context.Message.DeleteAsync ();
                        await user.RemoveRoleAsync (picdump);
                        var embed = new EmbedBuilder ();
                        embed.WithColor (Color.Purple);
                        embed.Description = "Removed picdump role";
                        await ReplyAndDeleteAsync ("", false, embed.Build (), TimeSpan.FromSeconds (5))
                            .ConfigureAwait (false);
                    }
                }
            catch {
                await Context.Channel.SendMessageAsync ("Walla").ConfigureAwait (false);
            }
        }

        [Command ("japanese")]
        public async Task AssignJapanese () {
            var channel = Context.Channel.Id.ToString ();
            if (channel == "339380254097539072" || channel == "339383206669320192")
                try {
                    var user = Context.User as SocketGuildUser;
                    var altUser = Context.User as IGuildUser;
                    var japanese = Context.Guild.Roles.FirstOrDefault (r => r.Name == "Japanese");
                    var roleCheck = altUser.RoleIds.Contains (japanese.Id);
                    if (roleCheck == false) {
                        await Context.Message.DeleteAsync ().ConfigureAwait (false);
                        await user.AddRoleAsync (japanese).ConfigureAwait (false);
                        var embed = new EmbedBuilder ();
                        embed.WithColor (Color.Purple);
                        embed.Description = "Assigned Japanese role";
                        await ReplyAndDeleteAsync ("", false, embed.Build (), TimeSpan.FromSeconds (5))
                            .ConfigureAwait (false);
                    } else if (roleCheck) {
                        await Context.Message.DeleteAsync ();
                        await user.RemoveRoleAsync (japanese);
                        var embed = new EmbedBuilder ();
                        embed.WithColor (Color.Purple);
                        embed.Description = "Removed Japanese role";
                        await ReplyAndDeleteAsync ("", false, embed.Build (), TimeSpan.FromSeconds (5))
                            .ConfigureAwait (false);
                    }
                }
            catch {
                // Ignore
            }
        }
    }
}