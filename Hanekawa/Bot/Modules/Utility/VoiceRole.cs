using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Database;
using Hanekawa.Database.Tables;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Command.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Hanekawa.Bot.Modules.Utility
{
    [Name("Voice Role")]
    [Description("Link a voice channel to a role. Usable to make the text channel only visible while in VC")]
    public class VoiceRole : HanekawaCommandModule
    {
        [Name("Add/Remove Linked Role")]
        [Command("vrole")]
        [Description("Adds channel user is connected to for the specified role")]
        [RequireMemberGuildPermissions(Permission.ManageChannels)]
        public async Task AddAsync([Remainder] CachedRole role)
        {
            if (Context.Member.VoiceState == null)
            {
                await ReplyAsync("Couldn't link role to a VC as you didn't supply a VC or connected to one", Color.Red);
                return;
            }
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var check = await db.VoiceRoles.FindAsync(Context.Guild.Id.RawValue,
                Context.Member.VoiceState.ChannelId.RawValue);
            if (check == null)
            {
                await db.VoiceRoles.AddAsync(new VoiceRoles
                {
                    GuildId = Context.Guild.Id.RawValue,
                    VoiceId = Context.Member.VoiceState.ChannelId.RawValue,
                    RoleId = role.Id.RawValue
                });
                await db.SaveChangesAsync();
                await ReplyAsync($"Added {role.Mention} as a VC role for {Context.Member.VoiceChannel.Name}!",
                    Color.Green);
                return;
            }

            if (Context.Member.VoiceState.ChannelId.RawValue != check.VoiceId)
            {
                check.VoiceId = Context.Member.VoiceState.ChannelId.RawValue;
                await db.SaveChangesAsync();
                await ReplyAsync($"Changed voice channel role to {Context.Member.VoiceChannel.Name}!", Color.Green);
                return;
            }

            db.VoiceRoles.Remove(check);
            await db.SaveChangesAsync();
            await ReplyAsync($"Removed {role.Mention} as VC role for {Context.Member.VoiceChannel.Name}!", Color.Green);
        }

        [Name("Add/Remove Linked Role")]
        [Command("vrole")]
        [Description("Adds voice channel and role as linked")]
        [RequireMemberGuildPermissions(Permission.ManageChannels)]
        public async Task AddAsync(CachedRole role, [Remainder] CachedVoiceChannel vc)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var check = await db.VoiceRoles.FindAsync(Context.Guild.Id.RawValue,
                vc.Id.RawValue);
            if (check == null)
            {
                await db.VoiceRoles.AddAsync(new VoiceRoles
                {
                    GuildId = Context.Guild.Id.RawValue,
                    VoiceId = vc.Id.RawValue,
                    RoleId = role.Id.RawValue
                });
                await db.SaveChangesAsync();
                await ReplyAsync($"Added {role.Mention} as a VC role for {vc.Name}!",
                    Color.Green);
                return;
            }

            if (vc.Id.RawValue != check.VoiceId)
            {
                check.VoiceId = vc.Id.RawValue;
                await db.SaveChangesAsync();
                await ReplyAsync($"Changed voice channel role to {vc.Name}!", Color.Green);
                return;
            }

            db.VoiceRoles.Remove(check);
            await db.SaveChangesAsync();
            await ReplyAsync($"Removed {Context.Guild.GetRole(check.RoleId)} as VC role for {vc.Name}!", Color.Green);
        }

        [Name("Remove Linked Role")]
        [Command("vrr")]
        [Description("Removes a role linked to mentioned voice channel")]
        [RequireMemberGuildPermissions(Permission.ManageChannels)]
        public async Task RemoveAsync([Remainder] CachedVoiceChannel channel)
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var check = await db.VoiceRoles.FindAsync(Context.Guild.Id.RawValue,
                channel.Id.RawValue);
            if (check == null)
            {
                await ReplyAsync("No role linked to this voice channel!", Color.Red);
                return;
            }

            db.VoiceRoles.Remove(check);
            await db.SaveChangesAsync();
            await ReplyAsync($"Removed {Context.Guild.GetRole(check.RoleId).Mention} from {channel.Name}!");
        }

        [Name("List Voice-Text linked channels")]
        [Command("vtlist")]
        [Description("Lists all voice channels linked to a role")]
        [RequireMemberGuildPermissions(Permission.ManageChannels)]
        public async Task ListAsync()
        {
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var list = await db.VoiceRoles.Where(x => x.GuildId == Context.Guild.Id.RawValue).ToListAsync();
            if (list == null || list.Count == 0)
            {
                await Context.ReplyAsync("No voice channel roles added");
                return;
            }

            var result = new List<string>();
            foreach (var x in list)
            {
                var role = Context.Guild.GetRole(x.RoleId) ??
                           Context.Guild.Roles.FirstOrDefault(z => z.Key.RawValue == x.RoleId).Value;
                var vc = Context.Guild.GetVoiceChannel(x.VoiceId) ??
                         Context.Guild.VoiceChannels.FirstOrDefault(e => e.Key.RawValue == x.VoiceId).Value;
                if (role != null) result.Add($"**{vc.Name} -> {role.Mention}**");
            }

            await Context.PaginatedReply(result, Context.Guild,
                $"Voice Channel Roles {Context.Guild.Name}");
        }
    }
}
