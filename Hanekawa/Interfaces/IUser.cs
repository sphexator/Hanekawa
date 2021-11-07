using System;

namespace Hanekawa.Interfaces
{
    public interface IUser
    {
        ulong Id { get; set; }
        string Name { get; set; }
        string Discriminator { get; set; }
        string AvatarUrl { get; set; }
        DateTimeOffset CreatedAt { get; set; }
    }
}