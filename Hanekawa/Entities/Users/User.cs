using System.ComponentModel.DataAnnotations;
using Hanekawa.Interfaces;

namespace Hanekawa.Entities.Users;

public class User : IEntity
{
    public ulong Id { get; set; }
    public DateTimeOffset? PremiumExpiration { get; set; }
    public List<GuildUser> GuildUsers { get; set; } = [];
    public List<Inventory> Inventory { get; set; } = [];
}

public class Equipment : IEntity
{
    public ulong Id { get; set; }

    public Guid BackgroundId { get; set; }
    public Inventory Background { get; set; } = null!;
}

public class Inventory
{
    public ulong UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid ItemId { get; set; }
    public Item Item { get; set; } = null!;
    public int Amount { get; set; }
}

public static class InventoryExtensions
{
    public static ICollection<Inventory> ToList(this ICollection<Inventory> inventory)
    {
        return new List<Inventory>(inventory);
    }
}

public class Item
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public Guid TypeId { get; set; }
    public ItemType Type { get; set; } = null!;
    public int? Price { get; set; }
}

public class ItemType
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public ICollection<Item> Items { get; set; } = new List<Item>();
}