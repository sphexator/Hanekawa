using System.ComponentModel.DataAnnotations;
using Hanekawa.Interfaces;

namespace Hanekawa.Entities.Users;

public class User : IEntity
{
    public ulong Id { get; set; }
    public DateTimeOffset? PremiumExpiration { get; set; }
    public List<GuildUser> GuildUsers { get; set; } = [];
}

public class Equipment : IEntity
{
    public ulong Id { get; set; }

    public Guid BackgroundId { get; set; }
    public Inventory Background { get; set; } = null!;
}

public class Inventory
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ItemId { get; set; }
    public Item Item { get; set; } = null!;
    public int Amount { get; set; } = 0;

    public ulong UserId { get; set; }
    public User User { get; set; } = null!;
}

public class Item
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int Price { get; set; } = 0;

    public ItemType Type { get; set; } = null!;
    public Guid TypeId { get; set; }
}

public class ItemType
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = null!;
    public bool Equippable { get; set; }
    public bool Consumable { get; set; }
    public bool Stackable { get; set; }
    public bool Sellable { get; set; }
    public bool Tradable { get; set; }
}