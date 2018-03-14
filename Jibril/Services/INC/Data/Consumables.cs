namespace Jibril.Services.INC.Data
{
    public class Consumables
    {
        public int TotalFood { get; set; }
        public int TotalDrink { get; set; }
        public int Beans { get; set; }
        public int Pasta { get; set; }
        public int Fish { get; set; }
        public int Ramen { get; set; }
        public int Coke { get; set; }
        public int Water { get; set; }
        public int MountainDew { get; set; }
        public int Redbull { get; set; }
        public int Bandages { get; set; }
    }

    public static class ConsumableNames
    {
        public const string Bandages = "bandages";

        public static readonly string[] Food =
        {
            "beans",
            "pasta",
            "fish",
            "ramen"
        };

        public static readonly string[] Water =
        {
            "coke",
            "water",
            "mountainDew",
            "redbull"
        };
    }
}