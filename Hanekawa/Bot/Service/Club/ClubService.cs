using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Hanekawa.Database;
using Hanekawa.Database.Entities;
using Hanekawa.Database.Tables.Club;
using Hanekawa.Entities;
using Hanekawa.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Hanekawa.Bot.Service.Club
{
    public class ClubService : INService
    {
        private readonly Hanekawa _bot;
        private readonly IServiceProvider _provider;
        private readonly Logger _logger;
        private readonly OverwritePermissions _denyOverwrite =
            new(ChannelPermissions.None, new ChannelPermissions(19520));
        private readonly OverwritePermissions _allowOverwrite =
            new(new ChannelPermissions(19520), ChannelPermissions.None);

        public ClubService(Hanekawa bot, IServiceProvider provider)
        {
            _bot = bot;
            _provider = provider;
            _logger = LogManager.GetCurrentClassLogger();
        }

        public async Task ReactionAddedAsync(ReactionAddedEventArgs e)
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
                await db.ClubPlayers.AddAsync(new ClubUser
                {
                    Id = Guid.NewGuid(),
                    Rank = ClubRank.Member,
                    GuildId = guild.Id,
                    JoinedAt = DateTimeOffset.UtcNow,
                    UserId = user.Id,
                    ClubId = club.Id,
                    Club = club
                });
                await db.SaveChangesAsync();
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception,
                    $"(Club Service) Error in {guild.Id} for reaction added - {exception.Message}");
            }
        }

        public async Task ReactionRemovedAsync(ReactionRemovedEventArgs e)
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
                db.ClubPlayers.Remove(clubUser);
                await db.SaveChangesAsync();
                _logger.Log(LogLevel.Info, $"(Club Service) {user.Id} left their club with reaction removed");
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception,
                    $"(Club Service) Error in {guild.Id} for reaction removed- {exception.Message}");
            }
        }
        
        public async Task MemberLeftAsync(MemberLeftEventArgs e)
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
        // TODO: Finish clubs
    }
}