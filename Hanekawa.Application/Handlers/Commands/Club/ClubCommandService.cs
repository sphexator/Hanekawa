using System;
using System.Linq;
using System.Threading.Tasks;
using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Interfaces.Commands;
using Hanekawa.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hanekawa.Application.Handlers.Commands.Club;

/// <inheritdoc />
public class ClubCommandService(IDbContext db) : IClubCommandService
{
    /// <inheritdoc />
    public async Task<Response<Message>> Create(ulong guildId, string name, string description, ulong authorId)
    {
        var existingClub = await db.Clubs.FirstOrDefaultAsync(c => c.GuildId == guildId && c.Name == name);
        if (existingClub != null)
        {
            return new Response<Message>(new Message($"A club with the name '{name}' already exists"));
        }

        // Check if user already owns a club
        var existingOwnership = await db.Clubs.AnyAsync(c => c.GuildId == guildId && c.OwnerId == authorId);
        if (existingOwnership)
        {
            return new Response<Message>(new Message("You already own a club. You can only own one club at a time."));
        }

        var club = new Entities.Club.Club
        {
            GuildId = guildId,
            Name = name,
            Description = description,
            OwnerId = authorId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Add owner as a member
        var member = new Entities.Club.ClubMember
        {
            GuildId = guildId,
            ClubName = name,
            UserId = authorId,
            JoinedAt = DateTimeOffset.UtcNow
        };

        await db.Clubs.AddAsync(club);
        await db.ClubMembers.AddAsync(member);
        await db.SaveChangesAsync();

        return new Response<Message>(new Message($"Club '{name}' has been created successfully!"));
    }

    /// <inheritdoc />
    public async Task<Response<Message>> Delete(ulong guildId, string name, ulong authorId)
    {
        var club = await db.Clubs.FirstOrDefaultAsync(c => c.GuildId == guildId && c.Name == name);
        if (club == null)
        {
            return new Response<Message>(new Message($"Club '{name}' doesn't exist"));
        }

        if (club.OwnerId != authorId)
        {
            return new Response<Message>(new Message("You don't have permission to delete this club"));
        }

        db.Clubs.Remove(club);
        await db.SaveChangesAsync();

        return new Response<Message>(new Message($"Club '{name}' has been deleted successfully"));
    }

    /// <inheritdoc />
    public async Task<Response<Message>> List(ulong guildId)
    {
        var clubs = await db.Clubs
            .Where(c => c.GuildId == guildId)
            .Select(c => new { c.Name, c.Description, MemberCount = c.Members.Count })
            .ToListAsync();

        if (!clubs.Any())
        {
            return new Response<Message>(new Message("No clubs found in this server"));
        }

        var message = "**Clubs in this server:**\n";
        foreach (var club in clubs)
        {
            message += $"- **{club.Name}** ({club.MemberCount} members): {club.Description}\n";
        }

        return new Response<Message>(new Message(message));
    }

    /// <inheritdoc />
    public async Task<Response<Message>> Join(ulong guildId, string name, ulong authorId)
    {
        var club = await db.Clubs
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.GuildId == guildId && c.Name == name);

        if (club == null)
        {
            return new Response<Message>(new Message($"Club '{name}' doesn't exist"));
        }

        if (club.Members.Any(m => m.UserId == authorId))
        {
            return new Response<Message>(new Message("You are already a member of this club"));
        }

        var member = new Entities.Club.ClubMember
        {
            GuildId = guildId,
            ClubName = name,
            UserId = authorId,
            JoinedAt = DateTimeOffset.UtcNow
        };

        await db.ClubMembers.AddAsync(member);
        await db.SaveChangesAsync();

        return new Response<Message>(new Message($"You have joined the club '{name}'"));
    }

    /// <inheritdoc />
    public async Task<Response<Message>> Leave(ulong guildId, string name, ulong authorId)
    {
        var club = await db.Clubs
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.GuildId == guildId && c.Name == name);

        if (club == null)
        {
            return new Response<Message>(new Message($"Club '{name}' doesn't exist"));
        }

        if (club.OwnerId == authorId)
        {
            return new Response<Message>(new Message("As the owner, you cannot leave your club. You must delete it or transfer ownership first."));
        }

        var membership = club.Members.FirstOrDefault(m => m.UserId == authorId);
        if (membership == null)
        {
            return new Response<Message>(new Message("You are not a member of this club"));
        }

        db.ClubMembers.Remove(membership);
        await db.SaveChangesAsync();

        return new Response<Message>(new Message($"You have left the club '{name}'"));
    }

    /// <inheritdoc />
    public async Task<Response<Message>> Info(ulong guildId, string name)
    {
        var club = await db.Clubs
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.GuildId == guildId && c.Name == name);

        if (club == null)
        {
            return new Response<Message>(new Message($"Club '{name}' doesn't exist"));
        }

        var message = $"**Club: {club.Name}**\n" +
                      $"Description: {club.Description}\n" +
                      $"Owner: <@{club.OwnerId}>\n" +
                      $"Created: {club.CreatedAt:yyyy-MM-dd}\n" +
                      $"Members: {club.Members.Count}\n\n" +
                      "**Member List:**\n";

        foreach (var member in club.Members.OrderBy(m => m.JoinedAt))
        {
            var joinedDate = member.JoinedAt.ToString("yyyy-MM-dd");
            var ownerIndicator = member.UserId == club.OwnerId ? " 👑" : "";
            message += $"- <@{member.UserId}> (joined: {joinedDate}){ownerIndicator}\n";
        }

        return new Response<Message>(new Message(message));
    }
}