using Hanekawa.Entities.Configs;
using Hanekawa.Entities.Discord;
using OneOf;
using OneOf.Types;

namespace Hanekawa.Application.Interfaces.Commands;

/// <summary>
/// 
/// </summary>
public interface IGreetService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="channel"></param>
    /// <returns></returns>
    Task<string> SetChannel(ulong guildId, TextChannel channel);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    Task<string> SetMessage(ulong guildId, string message);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="url"></param>
    /// <param name="uploaderId">User uploading image</param>
    /// <returns></returns>
    Task<string> SetImage(ulong guildId, string url, ulong uploaderId);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="guildId"></param>
    /// <returns></returns>
    Task<OneOf<NotFound, List<GreetImage>>> ListImages(ulong guildId);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<bool> RemoveImage(ulong guildId, int id);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="guildId"></param>
    /// <returns></returns>
    Task<string> ToggleImage(ulong guildId);
}