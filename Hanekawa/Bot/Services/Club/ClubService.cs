using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Events;
using Hanekawa.Database;
using Hanekawa.Database.Extensions;
using Hanekawa.Shared.Command;
using Hanekawa.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hanekawa.Bot.Services.Club
{
    public partial class ClubService : INService, IRequired
    {
        private readonly DiscordBot _client;
        private readonly InternalLogService _log;
        private readonly Random _random;
        private readonly IServiceProvider _provider;
        private readonly ColourService _colourService;

        private readonly OverwritePermissions _denyOverwrite = new OverwritePermissions(ChannelPermissions.None, new ChannelPermissions(19520));
        private readonly OverwritePermissions _allowOverwrite = new OverwritePermissions(new ChannelPermissions(19520), ChannelPermissions.None);

        public ClubService(DiscordBot client, Random random, InternalLogService log, IServiceProvider provider, ColourService colourService)
        {
            _client = client;
            _random = random;
            _log = log;
            _provider = provider;
            _colourService = colourService;

            _client.ReactionAdded += ClubReactionAdded;
            _client.ReactionRemoved += ClubReactionRemoved;
            _client.MemberLeft += ClubUserLeft;
        }

        private Task ClubUserLeft(MemberLeftEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                var user = e.User;
                var guild = e.Guild;
                try
                {
                    using (var db = new DbService())
                    {
                        var clubs = await db.ClubPlayers.Where(x => x.GuildId == guild.Id && x.UserId == user.Id)
                            .ToListAsync();
                        if (clubs.Count == 0) return;
                        var cfg = await db.GetOrCreateClubConfigAsync(guild);
                        foreach (var x in clubs) await RemoveUserAsync(user, guild, x.Id, db, cfg);
                        _log.LogAction(LogLevel.Information, $"(Club Service) {user.Id} left {guild.Id} and left {clubs.Count} clubs");
                    }
                    
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e,
                        $"(Club Service) Error in {guild.Id} for User Left- {e.Message}");
                }
            });
            return Task.CompletedTask;
        }

        private Task ClubReactionRemoved(ReactionRemovedEventArgs e) =>
            ClubJoinLeave(e.Message, e.Channel, e.User, e.Emoji, e.Reaction);

        private Task ClubReactionAdded(ReactionAddedEventArgs e) =>
            ClubJoinLeave(e.Message, e.Channel, e.User, e.Emoji, e.Reaction);

        private Task ClubJoinLeave(FetchableSnowflakeOptional<IMessage> msg, ICachedMessageChannel ch, FetchableSnowflakeOptional<IUser> usr, IEmoji emoji, Optional<ReactionData> reaction)
        {
            _ = Task.Run(async () =>
            {
                // if (!reaction.Emote.Equals(new Emoji("\u2714"))) return;
                // if (!(reaction.User.GetValueOrDefault() is SocketGuildUser user)) return;
                if (emoji.Name != "star") return;
                if (!usr.HasValue) return;
                var user = usr.Value;
                if (user.IsBot) return;
                if (!(ch is CachedTextChannel channel)) return;
                try
                {
                    using (var db = new DbService())
                    {
                        var cfg = await db.GetOrCreateClubConfigAsync(channel.Guild);
                        if (!cfg.AdvertisementChannel.HasValue) return;
                        if (cfg.AdvertisementChannel.Value != channel.Id) return;

                        var club = await db.ClubInfos.FirstOrDefaultAsync(x =>
                            x.GuildId == channel.Guild.Id && x.AdMessage == msg.Id);
                        if (club == null || !club.Public) return;

                        var clubUser = await db.ClubPlayers.FirstOrDefaultAsync(x =>
                            x.UserId == user.Id && x.GuildId == channel.Guild.Id && x.ClubId == club.Id);
                        if (clubUser == null) await AddUserAsync(user as CachedMember, club.Id, db, cfg);
                        else await RemoveUserAsync(user as CachedUser, channel.Guild, club.Id, db, cfg);
                    }
                }
                catch (Exception e)
                {
                    _log.LogAction(LogLevel.Error, e,
                        $"(Club Service) Error in {channel.Guild.Id} for Reaction added or removed - {e.Message}");
                }
            });
            return Task.CompletedTask;
        }
    }
}