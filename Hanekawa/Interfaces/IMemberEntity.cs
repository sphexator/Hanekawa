namespace Hanekawa.Interfaces;

public interface IMemberEntity : IGuildEntity, IEntity 
{ }

public interface IGuildEntity
{
    public ulong GuildId { get; set; }
}

public interface IEntity
{
    public ulong Id { get; set; }
}