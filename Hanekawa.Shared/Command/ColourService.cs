using System.Collections.Concurrent;
using Disqord;
using Hanekawa.Shared.Interfaces;

namespace Hanekawa.Shared.Command
{
    public class ColourService : INService
    {
        private readonly ConcurrentDictionary<ulong, Color> _colours = new ConcurrentDictionary<ulong, Color>();

        public Color Get(ulong guildId)
            => _colours.TryGetValue(guildId, out var color) ? color : Color.Purple;

        public void AddOrUpdate(ulong guildId, Color color)
            => _colours.AddOrUpdate(guildId, color, (k, v) => color);

        public bool TryRemove(ulong guildId) => _colours.TryRemove(guildId, out _);
    }

    public enum HanaColor
    {
        Default,
        Green,
        Red,
        Blue,
        Purple,
        Pink,
        Yellow,
        Black,
        White,
        Brown,
        Orange,
        Aqua,
        Maroon,
        Olive,
        Gray,
        Silver,
        Fuchsia,
        Navy,
        Teal,
        Lime
    }

    public static class HanaBaseColor
    {
        public static Color Default() => new Color(155, 89, 182);
        public static Color Green() => new Color(46, 204, 113);
        public static Color Red() => new Color(255, 105, 97);
        public static Color Blue() => new Color(9, 132, 227);
        public static Color Purple() => new Color(108, 92, 231);
        public static Color Pink() => new Color(255, 159, 243);
        public static Color Yellow() => new Color(241, 196, 15);
        public static Color Black() => new Color(45, 52, 54);
        public static Color White() => new Color(255, 255, 255);
        public static Color Brown() => new Color(133, 96, 63);
        public static Color Orange() => new Color(243, 156, 18);
        public static Color Aqua() => new Color(98, 235, 250);
        public static Color Maroon() => new Color(125, 105, 108);
        public static Color Olive() => new Color(165, 166, 126);
        public static Color Gray() => new Color(207, 207, 196);
        public static Color Silver() => new Color(189, 195, 199);
        public static Color Fuchsia() => new Color(255, 0, 255);
        public static Color Navy() => new Color(65, 74, 187);
        public static Color Teal() => new Color(99, 183, 183);
        public static Color Lime() => new Color(119, 221, 119);
    }
}