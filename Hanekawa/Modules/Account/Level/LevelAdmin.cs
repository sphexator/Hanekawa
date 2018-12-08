using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Hanekawa.Addons.Database;
using Hanekawa.Addons.Database.Extensions;
using Hanekawa.Addons.Database.Tables.GuildConfig;
using Hanekawa.Extensions.Embed;
using Hanekawa.Preconditions;
using Hanekawa.Services.Level;
using Hanekawa.Services.Level.Util;
using Microsoft.EntityFrameworkCore;
using Quartz.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hanekawa.Modules.Account.Level
{
    [Group("level")]
    public class LevelAdmin : InteractiveBase
    {
        private readonly LevelingService _levelingService;
        private readonly Calculate _calculate;
        private readonly DbService _db;

        public LevelAdmin(LevelingService levelingService, Calculate calculate, DbService db)
        {
            _levelingService = levelingService;
            _calculate = calculate;
            _db = db;
        }

        [Command("setlevel")]
        [Summary("Toggles between level roles stacking or keep the highest earned one")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetLevel(SocketGuildUser user, uint level)
        {
            uint totalExp = 0;
            for (var i = 1; i < level + 1; i++)
            {
                totalExp += _calculate.GetServerLevelRequirement((uint) i);
            }

            var userdata = await _db.GetOrCreateUserData(user);
            userdata.Level = level;
            userdata.Exp = 0;
            userdata.TotalExp = totalExp;
            await _db.SaveChangesAsync();
            await Context.ReplyAsync($"Set {user.Mention} level to {level}", Color.Green.RawValue);
        }

        [Command("role stack", RunMode = RunMode.Async)]
        [Alias("stack", "rolestack", "rs")]
        [Summary("Toggles between level roles stacking or keep the highest earned one")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task LevelStack()
        {
            var cfg = await _db.GetOrCreateGuildConfig(Context.Guild);
            if (cfg.StackLvlRoles)
            {
                cfg.StackLvlRoles = false;
                await _db.SaveChangesAsync();
                await Context.ReplyAsync("Users will now only keep the highest earned role.",
                    Color.Green.RawValue);
            }
            else
            {
                cfg.StackLvlRoles = true;
                await _db.SaveChangesAsync();
                await Context.ReplyAsync("Level roles does now stack.",
                    Color.Green.RawValue);
            }
        }

        [Command("add", RunMode = RunMode.Async)]
        [Summary("Adds a role reward")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireBotPermission(ChannelPermission.ManageRoles)]
        public async Task LevelAdd(uint level, [Remainder] string roleName)
        {
            if (roleName.IsNullOrWhiteSpace()) return;
            var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == roleName);
            if (role == null)
            {
                await Context.ReplyAsync("Couldn\'t find a role by that name.", Color.Red.RawValue);
                return;
            }

            var check = await _db.LevelRewards.FindAsync(Context.Guild.Id, level);
            if (check != null)
            {
                await Context.ReplyAsync("There\'s already a level reward on that level.", Color.Red.RawValue);
                return;
            }

            var data = new LevelReward
            {
                GuildId = Context.Guild.Id,
                Level = level,
                Role = role.Id,
                Stackable = false
            };
            await _db.LevelRewards.AddAsync(data);
            await _db.SaveChangesAsync();
            await Context.ReplyAsync($"Added {role.Name} as a lvl{level} reward!", Color.Green.RawValue);
        }

        [Command("create", RunMode = RunMode.Async)]
        [Summary("Creates a role reward with given level and name")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireBotPermission(ChannelPermission.ManageRoles)]
        public async Task LevelCreate(uint level, [Remainder] string roleName)
        {
            if (roleName.IsNullOrWhiteSpace()) return;

            var role = await Context.Guild.CreateRoleAsync(roleName, GuildPermissions.None);
            var data = new LevelReward
            {
                GuildId = Context.Guild.Id,
                Level = level,
                Role = role.Id,
                Stackable = false
            };
            await _db.LevelRewards.AddAsync(data);
            await _db.SaveChangesAsync();
            await Context.ReplyAsync($"Successfully created and added {role.Name} as a level reward for level{level}!",
                Color.Green.RawValue);
        }

        [Command("remove", RunMode = RunMode.Async)]
        [Summary("Removes a role reward with given level")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireBotPermission(ChannelPermission.ManageRoles)]
        public async Task LevelRemove(uint level)
        {
            var check = await _db.LevelRewards.FindAsync(Context.Guild.Id, level);
            if (check == null)
            {
                await Context.ReplyAsync("There is no reward connected to that level.", Color.Red.RawValue);
                return;
            }

            var role = _db.LevelRewards.First(x => x.GuildId == Context.Guild.Id && x.Level == level);
            _db.LevelRewards.Remove(role);
            await _db.SaveChangesAsync();
            await Context.ReplyAsync(
                $"Removed {Context.Guild.Roles.First(x => x.Id == role.Role).Name} from level rewards!",
                Color.Green.RawValue);
        }

        [Command("list", RunMode = RunMode.Async)]
        [Summary("Lists all role rewards")]
        [RequiredChannel]
        [Ratelimit(1, 5, Measure.Seconds)]
        public async Task LevelAdd()
        {
            var pages = new List<string>();
            foreach (var x in await _db.LevelRewards.Where(x => x.GuildId == Context.Guild.Id).OrderBy(x => x.Level)
                .ToListAsync())
            {
                pages.Add($"Name: {Context.Guild.GetRole(x.Role).Name ?? "Role not found"}\n" +
                          $"Level: {x.Level}\n" +
                          $"Stack: {x.Stackable}\n" +
                          "\n");
            }

            if (pages.Count != 0)
                await PagedReplyAsync(pages.PaginateBuilder(Context.Guild, $"Level roles for {Context.Guild.Name}"));
            else await Context.ReplyAsync("No level roles added!");
        }
    }
}