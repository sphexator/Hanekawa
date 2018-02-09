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

    public class ConsumableNames
    {
        public static string[] Food =
        {
            "Beans",
            "Pasta",
            "Fish",
            "Ramen"
        };

        public static string[] Water =
        {
            "Coke",
            "Water",
            "MountainDew",
            "Redbull"
        };

        public static string Bandages = "Bandages";
    }
}