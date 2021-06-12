using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Hosting;
using Disqord.Rest;
using Hanekawa.Database;
using Hanekawa.Database.Entities;
using Hanekawa.Database.Tables.Club;
using Hanekawa.Database.Tables.Config.Guild;
using Hanekawa.Extensions;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Hanekawa.Bot.Service.Club
{
    public class ClubService : DiscordClientService
    {
        private readonly Hanekawa _bot;
        private readonly IServiceProvider _provider;
        private readonly Logger _logger;
        private readonly OverwritePermissions _denyOverwrite =
            new(ChannelPermissions.None, new ChannelPermissions(19520));
        private readonly OverwritePermissions _allowOverwrite =
            new(new ChannelPermissions(19520), ChannelPermissions.None);

        public ClubService(IServiceProvider provider, ILogger logger, DiscordClientBase client) : base(logger, client)
        {
            _bot = (Hanekawa)client;
            _provider = provider;
            _logger = LogManager.GetCurrentClassLogger();
        }

        public async Task CreateChannelAsync(IGuild guild, ClubConfig cfg, Database.Tables.Club.Club club, DbService db)
        {
            if (!cfg.ChannelCategory.HasValue) return;
            if (guild.GetChannel(cfg.ChannelCategory.Value) is not ICategoryChannel channel)
            {
                cfg.ChannelCategory = null;
                await db.SaveChangesAsync();
                return;
            }

            var overWrites = new List<LocalOverwrite>
            {
                new (guild.Roles.FirstOrDefault(x => x.Value.Name.ToLower() == "everyone").Key,
                    OverwriteTargetType.Role, _denyOverwrite)
            };
            foreach (var x in await db.ClubPlayers.Where(x => x.ClubId == club.Id).ToArrayAsync()) 
                overWrites.Add(new LocalOverwrite(x.UserId, OverwriteTargetType.Member, _allowOverwrite));

            var clubChannel = await guild.CreateTextChannelAsync(club.Name, e =>
            {
                e.ParentId = cfg.ChannelCategory.Value;
                e.Overwrites = overWrites;
            });
            club.Channel = clubChannel.Id;
            await db.SaveChangesAsync();
        }

        public async Task AddPermissionsAsync(ITextChannel channel, IMember user)
        {
            var newList = new List<LocalOverwrite>
            {
                new (user.Id, OverwriteTargetType.Member, _allowOverwrite)
            };
            foreach (var xOverwrite in channel.Overwrites) 
                newList.Add(new LocalOverwrite(xOverwrite.TargetId, xOverwrite.TargetType, xOverwrite.Permissions));
            
            await channel.ModifyAsync(x =>
            {
                x.Overwrites = newList;
            });
        }

        public async Task RemovePermissionsAsync(ITextChannel channel, IMember user)
        {
            var newList = new List<LocalOverwrite>();
            foreach (var xOverwrite in channel.Overwrites)
            {
                if(xOverwrite.TargetId != user.Id) newList.Add(new LocalOverwrite(xOverwrite.TargetId, xOverwrite.TargetType, xOverwrite.Permissions));
            }
            
            await channel.ModifyAsync(x =>
            {
                x.Overwrites = newList;
            });
        }
        
        public async Task AddAsync(Snowflake userId, Snowflake guildId, Database.Tables.Club.Club club, DbService db)
        {
            await db.ClubPlayers.AddAsync(new ClubUser
            {
                Id = Guid.NewGuid(),
                Rank = ClubRank.Member,
                GuildId = guildId,
                JoinedAt = DateTimeOffset.UtcNow,
                UserId = userId,
                ClubId = club.Id,
                Club = club
            });
            await db.SaveChangesAsync();
            if (club.Channel.HasValue)
                await AddPermissionsAsync(_bot.GetChannel(guildId, club.Channel.Value) as ITextChannel,
                    _bot.GetMember(guildId, userId));
        }

        public async Task RemoveAsync(Snowflake userId, Snowflake guildId, Guid clubId, DbService db)
        {
            var club = await db.ClubInfos.FindAsync(clubId);
            if (club == null) return;
            await RemoveAsync(userId, guildId, club, db);
        }
        
        public async Task RemoveAsync(Snowflake userId, Snowflake guildId, Database.Tables.Club.Club club, DbService db)
        {
            var clubUser = await db.ClubPlayers.Include(x => x.Club)
                .FirstOrDefaultAsync(x => x.GuildId == guildId && x.UserId == userId);
            if (clubUser == null) return;
            await RemoveAsync(clubUser, club, db);
        }

        public async Task RemoveAsync(ClubUser clubUser, Database.Tables.Club.Club club, DbService db)
        {
            if (clubUser.Rank == ClubRank.Owner)
            {
                var users = await db.ClubPlayers.Where(x => x.ClubId == club.Id && x.Rank == ClubRank.Admin).ToArrayAsync();
                if (users.Length <= 0)
                    users = await db.ClubPlayers.Where(x => x.ClubId == club.Id).ToArrayAsync();
                if (users.Length > 1 == false)
                {
                    db.ClubPlayers.RemoveRange(users);
                    db.ClubInfos.Remove(club);
                    if (club.Channel.HasValue)
                        await _bot.GetGuild(club.GuildId).GetChannel(club.Channel.Value).DeleteAsync();
                }
                else
                {
                    db.ClubPlayers.Remove(clubUser);
                    if (club.Channel.HasValue)
                        await RemovePermissionsAsync(_bot.GetChannel(club.GuildId, club.Channel.Value) as ITextChannel,
                            _bot.GetMember(clubUser.GuildId, clubUser.UserId));
                }
            }
            else
            {
                db.ClubPlayers.Remove(clubUser);
                if (club.Channel.HasValue)
                    await RemovePermissionsAsync(_bot.GetChannel(club.GuildId, club.Channel.Value) as ITextChannel,
                        _bot.GetMember(clubUser.GuildId, clubUser.UserId));
            }   
            
            await db.SaveChangesAsync();
        }
        
        protected override async ValueTask OnReactionAdded(ReactionAddedEventArgs e)
        {
            if (!e.GuildId.HasValue) return;
            var guild = _bot.GetGuild(e.GuildId.Value);
            try
            {
                var user = await guild.GetOrFetchMemberAsync(e.UserId);
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var club = await db.ClubInfos.FirstOrDefaultAsync(x => x.AdMessage.Value == e.MessageId);
                if (club == null) return;
                var clubCheck =
                    await db.ClubPlayers.FirstOrDefaultAsync(x => x.GuildId == guild.Id && x.UserId == user.Id);
                if (clubCheck != null) return;
                await AddAsync(user.Id, guild.Id, club, db);
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception,
                    $"(Club Service) Error in {guild.Id} for reaction added - {exception.Message}");
            }
        }

        protected override async ValueTask OnReactionRemoved(ReactionRemovedEventArgs e)
        {
            if (!e.GuildId.HasValue) return;
            var guild = _bot.GetGuild(e.GuildId.Value);
            try
            {
                var user = await guild.GetOrFetchMemberAsync(e.UserId);
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var clubUser = await db.ClubPlayers.Include(x => x.Club)
                    .FirstOrDefaultAsync(x => x.GuildId == e.GuildId && x.UserId == e.UserId);
                if (clubUser?.Club.AdMessage == null) return;
                if (clubUser.Club.AdMessage != null && e.MessageId != clubUser.Club.AdMessage.Value) return;
                if (clubUser.Rank == ClubRank.Owner) return;
                await RemoveAsync(user.Id, user.GuildId, clubUser.ClubId, db);
                await db.SaveChangesAsync();
                _logger.Log(LogLevel.Info, $"(Club Service) {user.Id} left their club with reaction removed");
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception,
                    $"(Club Service) Error in {guild.Id} for reaction removed- {exception.Message}");
            }
        }

        protected override async ValueTask OnMemberLeft(MemberLeftEventArgs e)
        {
            var user = e.User;
            var guild = _bot.GetGuild(e.GuildId);
            try
            {
                using var scope = _provider.CreateScope();
                await using var db = scope.ServiceProvider.GetRequiredService<DbService>();
                var clubUser = await db.ClubPlayers.Include(x => x.Club)
                    .FirstOrDefaultAsync(x => x.GuildId == e.GuildId && x.UserId == e.User.Id);
                if (clubUser == null) return;
                db.ClubPlayers.Remove(clubUser);
                await db.SaveChangesAsync();
                _logger.Log(LogLevel.Info, $"(Club Service) {user.Id} left {guild.Id} and left their club");
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception,
                    $"(Club Service) Error in {guild.Id} for User Left- {exception.Message}");
            }
        }
    }
}