using System.Threading.Tasks;
using Hanekawa.Entities;

namespace Hanekawa.Application.Interfaces.Commands;

/// <summary>
/// Service for managing clubs in a guild
/// </summary>
public interface IClubCommandService
{
    /// <summary>
    /// Creates a new club in the guild
    /// </summary>
    /// <param name="guildId">The ID of the guild</param>
    /// <param name="name">The name of the club</param>
    /// <param name="description">The description of the club</param>
    /// <param name="authorId">The ID of the user creating the club</param>
    /// <returns>A message indicating success or failure</returns>
    Task<Response<Message>> Create(ulong guildId, string name, string description, ulong authorId);
    
    /// <summary>
    /// Deletes a club from the guild
    /// </summary>
    /// <param name="guildId">The ID of the guild</param>
    /// <param name="name">The name of the club</param>
    /// <param name="authorId">The ID of the user deleting the club</param>
    /// <returns>A message indicating success or failure</returns>
    Task<Response<Message>> Delete(ulong guildId, string name, ulong authorId);
    
    /// <summary>
    /// Lists all clubs in the guild
    /// </summary>
    /// <param name="guildId">The ID of the guild</param>
    /// <returns>A message with the list of clubs</returns>
    Task<Response<Message>> List(ulong guildId);
    
    /// <summary>
    /// Joins a club in the guild
    /// </summary>
    /// <param name="guildId">The ID of the guild</param>
    /// <param name="name">The name of the club</param>
    /// <param name="authorId">The ID of the user joining the club</param>
    /// <returns>A message indicating success or failure</returns>
    Task<Response<Message>> Join(ulong guildId, string name, ulong authorId);
    
    /// <summary>
    /// Leaves a club in the guild
    /// </summary>
    /// <param name="guildId">The ID of the guild</param>
    /// <param name="name">The name of the club</param>
    /// <param name="authorId">The ID of the user leaving the club</param>
    /// <returns>A message indicating success or failure</returns>
    Task<Response<Message>> Leave(ulong guildId, string name, ulong authorId);
    
    /// <summary>
    /// Gets information about a club
    /// </summary>
    /// <param name="guildId">The ID of the guild</param>
    /// <param name="name">The name of the club</param>
    /// <returns>A message with information about the club</returns>
    Task<Response<Message>> Info(ulong guildId, string name);
}