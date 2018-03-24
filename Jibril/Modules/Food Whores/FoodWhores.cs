using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jibril.Modules.Food_Whores
{
        public class FoodWhores : ModuleBase<SocketCommandContext>
        {
            [Command("food")]
            public async Task FoodTaskAsync()
            {
                var food = GetFood().FirstOrDefault();

                var wtf = new EmbedBuilder();

                wtf.WithTitle("Food Whores Anonymous")
                    .WithDescription($"Your food is {food.Name}, {Context.User.Mention}")
                    .WithImageUrl(imageUrl: $"{food.Image}")
                    .WithFooter($"{food.Fooddesc}")
                    .WithColor(Color.LighterGrey);
                {
                    await ReplyAsync("", false, wtf.Build());
                }
            }
            private static List<FoodTest1> GetFood()
            {
                var r = new Random();
                var food = new List<string> { "Pasta", "Cookies" };  //"Pizza", "Salad", "Sushi", "Fruit", "Vegetables", "Canned Food", "Tacos", "Pancakes", "Coffee", "Soda", "Tea", "Ice Cream" };
                var choose = r.Next(food.Count);
                var result = new List<FoodTest1>();
                switch (food[choose])
                {
                    case "Pasta":
                        result.Add(new FoodTest1
                        {
                            Image = Pasta(),
                            Name = food[choose],
                            Fooddesc = "Pasta is a type of noodle that is used in cooking. It is a staple food of most Italian cuisines. The first reference to pasta, in a book, was in 1154. There are more than 600 pasta shapes produced worldwide."
                        });
                        break;
                    case "Cookies":
                        result.Add(new FoodTest1
                        {
                            Image = Cookies(),
                            Name = food[choose],
                            Fooddesc = "A cookie is a baked or cooked food that is small, flat and sweet. It usually contains flour, sugar and some type of oil or fat. It may include other ingredients such as raisins, oats, chocolate chips, nuts, etc."
                        });
                        break;
                }
                return result;
            }
            private static string Pasta()
            {
                var cookies = new List<string>();
                cookies.Add("https://i.imgur.com/N14Gfrk.png");
                cookies.Add("https://i.imgur.com/DmFgBTk.jpg");
                cookies.Add("https://imgur.com/lWuOzX5.jpg");
                cookies.Add("https://i.imgur.com/Ezg6tr4.png");
                cookies.Add("https://i.imgur.com/hvSqX6G.png");

                // { "https://i.imgur.com/N14Gfrk.png", "https://imgur.com/lWuOzX5.jpg", "https://i.imgur.com/DmFgBTk.jpg", "https://i.imgur.com/Ezg6tr4.png", "https://i.imgur.com/hvSqX6G.png" };

                var r = new Random();
                var cookie = r.Next(cookies.Count);
                return cookies[cookie];
            }
            private static string Cookies()
            {
                var cookies = new List<string>();
                cookies.Add("https://i.imgur.com/obId3uA.png");
                cookies.Add("https://i.imgur.com/OCyTsTZ.png");
                cookies.Add("https://i.imgur.com/HybEZYH.png");
                cookies.Add("https://i.imgur.com/aCkF8Lf.png");
                cookies.Add("https://i.imgur.com/Gq83Qht.png");


                //{ "https://i.imgur.com/obId3uA.png", "https://i.imgur.com/OCyTsTZ.png", "https://i.imgur.com/HybEZYH.png", "https://i.imgur.com/aCkF8Lf.png", "https://i.imgur.com/Gq83Qht.png" };

                var a = new Random();
                var cookie = a.Next(cookies.Count);
                return cookies[cookie];
            }
        }
        public class FoodTest1
        {
            public string Name { get; set; }
            public string Image { get; set; }
            public string Fooddesc { get; set; }
        }
}
