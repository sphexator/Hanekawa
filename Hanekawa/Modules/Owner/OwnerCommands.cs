using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Hanekawa.Addons.Database;
using Hanekawa.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Modules.Owner
{
    [RequireOwner]
    public class OwnerCommands : InteractiveBase
    {
        [Command("quit")]
        public async Task ExitProgramAsync()
        {
            await ReplyAsync(null, false, new EmbedBuilder().Reply("Exiting...").Build());
            Environment.Exit(0);
        }

        [Group("blacklist")]
        public class Blacklist : InteractiveBase
        {
            [Command("add", RunMode = RunMode.Async)]
            [RequireOwner]
            public async Task BlackListAddAsync(ulong id, [Remainder] string reason = null)
            {
                using (var db = new DbService())
                {
                    var check = await db.Blacklists.FindAsync(id);
                    if (check != null)
                    {
                        await ReplyAsync(null, false,
                            new EmbedBuilder().Reply("Id is already blacklisted", Color.Red.RawValue).Build());
                        return;
                    }

                    var data = new Addons.Database.Tables.Administration.Blacklist
                    {
                        GuildId = id,
                        Reason = reason,
                        Unban = null,
                        ResponsibleUser = Context.User.Id
                    };

                    await db.Blacklists.AddAsync(data);
                    await db.SaveChangesAsync();

                    var checkTwo = Context.Client.Guilds.FirstOrDefault(x => x.Id == id);
                    if (checkTwo != null) await checkTwo.LeaveAsync();

                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Added {id} to the server blacklist", Color.Green.RawValue).Build());
                }
            }

            [Command("remove", RunMode = RunMode.Async)]
            [RequireOwner]
            public async Task BlackListRemoveAsync(ulong id)
            {
                using (var db = new DbService())
                {
                    var check = await db.Blacklists.FindAsync(id);
                    if (check is null)
                    {
                        await ReplyAsync(null, false,
                            new EmbedBuilder().Reply("Id isn't blacklisted", Color.Red.RawValue).Build());
                        return;
                    }

                    var data = await db.Blacklists.FirstOrDefaultAsync(x => x.GuildId == id);
                    db.Blacklists.Remove(data);
                    await db.SaveChangesAsync();
                    await ReplyAsync(null, false,
                        new EmbedBuilder().Reply($"Removed {id} from the server blacklist", Color.Green.RawValue)
                            .Build());
                }
            }
        }
    }
}