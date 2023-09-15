using Hanekawa.Entities;

namespace Hanekawa.Application.Interfaces.Commands;

/// <summary>
/// 
/// </summary>
public interface IClubCommandService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="authorId"></param>
    /// <returns></returns>
    Task<Response<Message>> Create(ulong guildId, string name, string description, ulong authorId);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="name"></param>
    /// <param name="authorId"></param>
    /// <returns></returns>
    Task<Response<Message>> Delete(ulong guildId, string name, ulong authorId);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="guildId"></param>
    /// <returns></returns>
    Task<Response<Message>> List(ulong guildId);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="name"></param>
    /// <param name="authorId"></param>
    /// <returns></returns>
    Task<Response<Message>> Join(ulong guildId, string name, ulong authorId);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="name"></param>
    /// <param name="authorId"></param>
    /// <returns></returns>
    Task<Response<Message>> Leave(ulong guildId, string name, ulong authorId);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    Task<Response<Message>> Info(ulong guildId, string name);
}