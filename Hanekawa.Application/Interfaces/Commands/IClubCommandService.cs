using Hanekawa.Entities;

namespace Hanekawa.Application.Interfaces.Commands;

public interface IClubCommandService
{
    Task<Response<Message>> Create(ulong guildId, string name, string description, ulong authorId);
    Task<Response<Message>> Delete(ulong guildId, string name, ulong authorId);
    Task<Response<Message>> List(ulong guildId);
    Task<Response<Message>> Join(ulong guildId, string name, ulong authorId);
    Task<Response<Message>> Leave(ulong guildId, string name, ulong authorId);
    Task<Response<Message>> Info(ulong guildId, string name);
}