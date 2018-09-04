using System;
using Discord;
using Discord.WebSocket;
using Hanekawa.Extensions;
using Hanekawa.Services.Entities;
using Hanekawa.Services.Entities.Tables;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Services.Club
{
    public class ClubService
    {
        public enum UpdateType { Image, Description}
        private readonly DiscordSocketClient _client;

        public ClubService(DiscordSocketClient client)
        {
            _client = client;

            _client.ReactionAdded += AddClubMemberAsync;
            _client.ReactionRemoved += RemoveClubMemberAsync;
            _client.UserLeft += ClubRemoveAsync;
        }

        public async Task PostAdvertisementAsync(DbService db, GuildConfig cfg, IGuild guild, ClubInfo club)
        {
            var embed = new EmbedBuilder
            {
                Color = Color.Purple,
                Author = new EmbedAuthorBuilder { Name = club.Name},
                ImageUrl = club.ImageUrl,
                Description = club.Description ?? "No description added"
            };
            var msg = await (await guild.GetTextChannelAsync(cfg.ClubAdvertisementChannel.Value)).SendMessageAsync(null,
                false, embed.Build());
            club.AdMessage = msg.Id;
            await db.SaveChangesAsync();
            if (club.Public) await msg.AddReactionAsync(new Emoji("\u2714"));
        }

        public async Task UpdatePostAsync(GuildConfig cfg, IUserMessage msg, ClubInfo club, UpdateType type, string content)
        {
            switch (type)
            {
                case UpdateType.Description:
                    var embedDesc = msg.Embeds.First().ToEmbedBuilder();
                    embedDesc.Description = content;
                    await msg.ModifyAsync(x => x.Embed = embedDesc.Build());
                    break;
                case UpdateType.Image:
                    var embedImg = msg.Embeds.First().ToEmbedBuilder();
                    embedImg.ImageUrl = content;
                    await msg.ModifyAsync(x => x.Embed = embedImg.Build());
                    break;
            }
        }

        public async Task UpdatePostAsync(GuildConfig cfg, IGuild guild, ClubInfo club, UpdateType type, string content)
        {
            if (!(await (await guild.GetTextChannelAsync(cfg.ClubAdvertisementChannel.Value)).GetMessageAsync(
                club.AdMessage.Value) is IUserMessage message)) return;

            switch (type)
            {
                case UpdateType.Description:
                    var embedDesc = message.Embeds.First().ToEmbedBuilder();
                    embedDesc.Description = content;
                    await message.ModifyAsync(x => x.Embed = embedDesc.Build());
                    break;
                case UpdateType.Image:
                    var embedImg = message.Embeds.First().ToEmbedBuilder();
                    embedImg.ImageUrl = content;
                    await message.ModifyAsync(x => x.Embed = embedImg.Build());
                    break;
            }
        }

        private static Task ClubRemoveAsync(SocketGuildUser user)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    var clubs = await db.ClubPlayers.Where(x => x.GuildId == user.Guild.Id && x.UserId == user.Id)
                        .ToListAsync();
                    if (clubs.Count == 0) return;
                    foreach (var x in clubs)
                    {
                        if (x.Rank == 1)
                        {
                            var toPromote = await db.ClubPlayers.FirstOrDefaultAsync(y =>
                                                y.GuildId == x.GuildId && y.ClubId == x.Id && y.Rank == 2) ?? await db.ClubPlayers.FirstOrDefaultAsync(y =>
                                                y.GuildId == x.GuildId && y.ClubId == x.Id && y.Rank == 3);
                            toPromote.Rank = 1;
                        }
                        db.ClubPlayers.Remove(x);
                    }

                    await db.SaveChangesAsync();
                }
            });
            return Task.CompletedTask;
        }

        private static Task RemoveClubMemberAsync(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    if (!(arg2 is ITextChannel channel)) return;
                    var cfg = await db.GetOrCreateGuildConfig(channel.Guild);
                    if (!cfg.ClubAdvertisementChannel.HasValue) return;
                    if (cfg.ClubAdvertisementChannel.Value != channel.Id) return;

                    var club = await db.ClubInfos.FirstOrDefaultAsync(x =>
                        x.GuildId == channel.Guild.Id && x.AdMessage == arg3.MessageId);
                    if (!club.Public || club == null) return;

                    var Clubuser = await db.ClubPlayers.FirstOrDefaultAsync(x =>
                        x.GuildId == channel.Guild.Id && x.ClubId == club.Id && x.UserId == arg3.UserId);
                    if (Clubuser == null) return;

                    var user = arg3.User.Value as IGuildUser;
                    if (!arg3.User.IsSpecified) user = await channel.Guild.GetUserAsync(arg3.UserId);
                    await user.RemoveRoleAsync(channel.Guild.GetRole(club.RoleId.Value));

                    db.ClubPlayers.Remove(Clubuser);
                    await db.SaveChangesAsync();
                }
            });
            return Task.CompletedTask;
        }

        private static Task AddClubMemberAsync(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            var _ = Task.Run(async () =>
            {
                using (var db = new DbService())
                {
                    if (!(arg2 is ITextChannel channel)) return;
                    var cfg = await db.GetOrCreateGuildConfig(channel.Guild);
                    if (!cfg.ClubAdvertisementChannel.HasValue) return;
                    if (cfg.ClubAdvertisementChannel.Value != channel.Id) return;

                    var club = await db.ClubInfos.FirstOrDefaultAsync(x =>
                        x.GuildId == channel.Guild.Id && x.AdMessage == arg3.MessageId);
                    if (!club.Public || club.AdMessage == null) return;

                    var Clubuser = await db.ClubPlayers.FirstOrDefaultAsync(x =>
                        x.GuildId == channel.Guild.Id && x.ClubId == club.Id && x.UserId == arg3.UserId);
                    if (Clubuser != null) return;

                    var user = arg3.User.Value as IGuildUser;
                    if (!arg3.User.IsSpecified) user = await channel.Guild.GetUserAsync(arg3.UserId);
                    await user.AddRoleAsync(channel.Guild.GetRole(club.RoleId.Value));

                    var data = new ClubPlayer
                    {
                        ClubId = club.Id,
                        GuildId = channel.Guild.Id,
                        JoinDate = DateTimeOffset.UtcNow,
                        Rank = 3,
                        UserId = user.Id,
                        Id = (await db.ClubPlayers.CountAsync(x => x.GuildId == channel.Guild.Id)) + 1
                    };
                    db.ClubPlayers.Add(data);
                    await db.SaveChangesAsync();
                }
            });
            return Task.CompletedTask;
        }

        private static readonly OverwritePermissions DenyOverwrite = new OverwritePermissions(
            addReactions: PermValue.Deny, sendMessages: PermValue.Deny, attachFiles: PermValue.Deny,
            embedLinks: PermValue.Deny, viewChannel: PermValue.Deny);

        private static readonly OverwritePermissions AllowOverwrite =
            new OverwritePermissions(addReactions: PermValue.Allow, sendMessages: PermValue.Allow,
                attachFiles: PermValue.Allow, embedLinks: PermValue.Allow, viewChannel: PermValue.Allow);

        public async Task<int> IsChannelRequirementAsync(DbService db, IEnumerable<ClubPlayer> users, uint level = 40)
        {
            return await GetUsersOfLevelAsync(db, level, users);
        }

        public async Task CreateChannelAsync(DbService db, IGuild guild, GuildConfig cfg, string clubName, IGuildUser leader,
            IEnumerable<ClubPlayer> users, ClubInfo club)
        {
            var channel = await guild.CreateTextChannelAsync(clubName, x => x.CategoryId = cfg.ClubChannelCategory);
            var role = await guild.CreateRoleAsync(clubName, GuildPermissions.None);
            await channel.AddPermissionOverwriteAsync(guild.EveryoneRole, DenyOverwrite);
            await channel.AddPermissionOverwriteAsync(role, AllowOverwrite);

            club.Channel = channel.Id;
            club.RoleId = role.Id;
            await db.SaveChangesAsync();

            await leader.AddRoleAsync(role);
            foreach (var x in users)
                try
                {
                    await (await guild.GetUserAsync(x.UserId)).AddRoleAsync(role);
                }
                catch
                {
                }
        }

        private static async Task<int> GetUsersOfLevelAsync(DbService db, uint level, IEnumerable<ClubPlayer> players)
        {
            var nr = 0;
            foreach (var x in players)
            {
                var user = await db.GetOrCreateUserData(x.GuildId, x.UserId);
                if (user.Level >= level) nr++;
            }

            return nr;
        }
    }
}