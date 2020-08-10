using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Hanekawa.Bot.Services.Club;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Command.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using Quartz.Util;

namespace Hanekawa.Bot.Modules.Club
{
    public partial class Club
    {
        [Name("Rename Club")]
        [Command("csn")]
        [Description("Force changes a name of a club")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task ClubForceRename(int clubId, [Remainder] string name)
        {
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var club = await db.ClubInfos.FirstOrDefaultAsync(x => x.Id == clubId && x.GuildId == Context.Guild.Id.RawValue);
            if (club == null)
            {
                await Context.ReplyAsync("There's no club with that ID in this guild");
                return;
            }

            if (name.IsNullOrWhiteSpace())
            {
                await Context.ReplyAsync("Please provide a proper name");
                return;
            }

            var cfg = await db.GetOrCreateClubConfigAsync(Context.Guild);
            club.Name = name;
            await db.SaveChangesAsync();
            if (club.AdMessage.HasValue && cfg.AdvertisementChannel.HasValue)
            {
                var msg = await Context.Guild.GetTextChannel(cfg.AdvertisementChannel.Value)
                    .GetMessageAsync(club.AdMessage.Value) as IUserMessage;
                await ClubService.UpdatePostNameAsync(msg, name);
            }

            if (club.Role.HasValue)
            {
                var role = Context.Guild.GetRole(club.Role.Value);
                await role.ModifyAsync(x => x.Name = name);
            }

            if (club.Channel.HasValue)
            {
                var channel = Context.Guild.GetTextChannel(club.Channel.Value);
                await channel.ModifyAsync(x => x.Name = name);
            }
        }

        [Name("Change Club Icon")]
        [Command("csicon")]
        [Description("Force changes icon of a club")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task ClubForceReIcon(int clubId, [Remainder] string icon)
        {
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var club = await db.ClubInfos.FirstOrDefaultAsync(x => x.Id == clubId && x.GuildId == Context.Guild.Id.RawValue);
            if (club == null)
            {
                await Context.ReplyAsync("There's no club with that ID in this guild");
                return;
            }

            var cfg = await db.GetOrCreateClubConfigAsync(Context.Guild);
            club.IconUrl = icon;
            await db.SaveChangesAsync();
            if (club.AdMessage.HasValue && cfg.AdvertisementChannel.HasValue)
            {
                var msg = await Context.Guild.GetTextChannel(cfg.AdvertisementChannel.Value)
                    .GetMessageAsync(club.AdMessage.Value) as IUserMessage;
                await ClubService.UpdatePostIconAsync(msg, icon);
            }
        }

        [Name("Change Club Image")]
        [Command("csi", "csimage")]
        [Description("Force changes image of a club")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task ClubForceReImage(int clubId, [Remainder] string image)
        {
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var club = await db.ClubInfos.FirstOrDefaultAsync(x => x.Id == clubId && x.GuildId == Context.Guild.Id.RawValue);
            if (club == null)
            {
                await Context.ReplyAsync("There's no club with that ID in this guild");
                return;
            }

            var cfg = await db.GetOrCreateClubConfigAsync(Context.Guild);
            club.ImageUrl = image;
            await db.SaveChangesAsync();
            if (club.AdMessage.HasValue && cfg.AdvertisementChannel.HasValue)
            {
                var msg = await Context.Guild.GetTextChannel(cfg.AdvertisementChannel.Value)
                    .GetMessageAsync(club.AdMessage.Value) as IUserMessage;
                await ClubService.UpdatePostImageAsync(msg, image);
            }
        }

        [Name("Change Club Description")]
        [Command("csd")]
        [Description("Force changes a name of a club")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task ClubForceReDescription(int clubId, [Remainder] string desc)
        {
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var club = await db.ClubInfos.FirstOrDefaultAsync(x => x.Id == clubId && x.GuildId == Context.Guild.Id.RawValue);
            if (club == null)
            {
                await Context.ReplyAsync("There's no club with that ID in this guild");
                return;
            }

            if (desc.IsNullOrWhiteSpace()) desc = "N/A";

            var cfg = await db.GetOrCreateClubConfigAsync(Context.Guild);
            club.Description = desc;
            await db.SaveChangesAsync();
            if (club.AdMessage.HasValue && cfg.AdvertisementChannel.HasValue)
            {
                var msg = await Context.Guild.GetTextChannel(cfg.AdvertisementChannel.Value)
                    .GetMessageAsync(club.AdMessage.Value) as IUserMessage;
                await ClubService.UpdatePostDescriptionAsync(msg, desc);
            }
        }

        [Name("Club Role Toggle")]
        [Command("crt")]
        [Description(
            "Toggles the use of creating roles for club or channel permission. Auto to channel when above 50 roles")]
        [RequireMemberGuildPermissions(Permission.ManageGuild)]
        public async Task ToggleClubRole()
        {
            
            await using var db = Context.Scope.ServiceProvider.GetRequiredService<DbService>();
            var cfg = await db.GetOrCreateClubConfigAsync(Context.Guild);
            if (cfg.RoleEnabled)
            {
                cfg.RoleEnabled = false;
                await db.SaveChangesAsync();
                await Context.ReplyAsync("Disabled creation of roles for clubs.\n" +
                                         "Now using channel permissions to add users to their designated channel");
            }
            else
            {
                cfg.RoleEnabled = true;
                await db.SaveChangesAsync();
                await Context.ReplyAsync("Enabled creation of roles for clubs.\n" +
                                         "Now using their designated role to add users to their channel");
            }
        }
    }
}