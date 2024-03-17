using Hanekawa.Application.Interfaces;
using Hanekawa.Application.Interfaces.Commands;
using Hanekawa.Entities;

namespace Hanekawa.Application.Handlers.Commands.Club;

/// <inheritdoc />
public class ClubCommandService(IDbContext db) : IClubCommandService
{
    /// <inheritdoc />
    public Task<Response<Message>> Create(ulong guildId, string name, string description, ulong authorId)
    {
        throw new NotImplementedException();
    }
    
    /// <inheritdoc />
    public Task<Response<Message>> Delete(ulong guildId, string name, ulong authorId)
    {
        throw new NotImplementedException();
    }
    
    /// <inheritdoc />
    public Task<Response<Message>> List(ulong guildId)
    {
        throw new NotImplementedException();
    }
    
    /// <inheritdoc />
    public Task<Response<Message>> Join(ulong guildId, string name, ulong authorId)
    {
        throw new NotImplementedException();
    }
    
    /// <inheritdoc />
    public Task<Response<Message>> Leave(ulong guildId, string name, ulong authorId)
    {
        throw new NotImplementedException();
    }
    
    /// <inheritdoc />
    public Task<Response<Message>> Info(ulong guildId, string name)
    {
        throw new NotImplementedException();
    }
}