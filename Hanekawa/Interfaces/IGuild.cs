using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hanekawa.Interfaces
{
    public interface IGuild
    {
        ulong Id { get; set; }
        string Name { get; set; }
        string IconUrl { get; set; }
        ulong OwnerId { get; set; }
        DateTimeOffset CreatedAt { get; set; }
        
        ValueTask<ulong> GetRole(ulong roleId);
        ValueTask<List<ulong>> GetRoles();
        ValueTask<IGuildUser> GetMember(ulong userId);
        ValueTask<List<IGuildUser>> GetMembers();
    }
}