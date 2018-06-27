using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
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
                var food = new List<string> { "Pasta", "Cookies", "Pizza", "Salad", "Sushi", "Fruit", "Vegetables", "Canned Food", "Tacos", "Pancakes", "Coffee", "Soda", "Tea", "Ice Cream", "Sandwich", };
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
                    case "Pizza":
                        result.Add(new FoodTest1
                        {
                            Image = Pizza(),
                            Name = food[choose],
                            Fooddesc = ""
                        });
                        break;
                    case "Salad":
                        result.Add(new FoodTest1
                        {
                            Image = Salad(),
                            Name = food[choose],
                            Fooddesc = ""
                        });
                        break;
                    case "Sushi":
                        result.Add(new FoodTest1
                        {
                            Image = Sushi(),
                            Name = food[choose],
                            Fooddesc = ""
                        });
                        break;
                    case "Fruit":
                        result.Add(new FoodTest1
                        {
                            Image = Fruit(),
                            Name = food[choose],
                            Fooddesc = ""
                        });
                        break;
                    case "Vegetables":
                        result.Add(new FoodTest1
                        {
                            Image = Vegetables(),
                            Name = food[choose],
                            Fooddesc = ""
                        });
                        break;
                    case "Canned Food":
                        result.Add(new FoodTest1
                        {
                            Image = CannedFood(),
                            Name = food[choose],
                            Fooddesc = ""
                        });
                        break;
                //"Tacos", "Pancakes", "Coffee", "Soda", "Tea", "Ice Cream"
                    case "Tacos":
                        result.Add(new FoodTest1
                        {
                            Image = Tacos(),
                            Name = food[choose],
                            Fooddesc = ""
                        });
                        break;
                    case "Pancakes":
                        result.Add(new FoodTest1
                        {
                            Image = Pancakes(),
                            Name = food[choose],
                            Fooddesc = ""
                        });
                        break;
                    case "Coffee":
                        result.Add(new FoodTest1
                        {
                            Image = Coffee(),
                            Name = food[choose],
                            Fooddesc = ""
                        });
                        break;
                    case "Soda":
                        result.Add(new FoodTest1
                        {
                            Image = Soda(),
                            Name = food[choose],
                            Fooddesc = ""
                        });
                        break;
                    case "Tea":
                        result.Add(new FoodTest1
                        {
                            Image = Tea(),
                            Name = food[choose],
                            Fooddesc = ""
                        });
                        break;
                    case "Ice Cream":
                        result.Add(new FoodTest1
                        {
                            Image = IceCream(),
                            Name = food[choose],
                            Fooddesc = ""
                        });
                        break;
                    case "Sandwich":
                        result.Add(new FoodTest1
                        {
                            Image = Sandwich(),
                            Name = food[choose],
                            Fooddesc = ""
                        });
                        break;

            }
            return result;
            }
            private static string Pasta()
            {
                var pasta = new List<string>();
                pasta.Add("https://i.imgur.com/N14Gfrk.png"); //1
                pasta.Add("https://i.imgur.com/DmFgBTk.jpg"); //2
                pasta.Add("https://imgur.com/lWuOzX5.jpg");   //3
                pasta.Add("https://i.imgur.com/Ezg6tr4.png"); //4
                pasta.Add("https://i.imgur.com/hvSqX6G.png"); //5
                pasta.Add(""); //6
                pasta.Add(""); //7
                pasta.Add(""); //8
                pasta.Add(""); //9
                pasta.Add(""); //10
                pasta.Add(""); //11
                pasta.Add(""); //12
                pasta.Add(""); //13
                pasta.Add(""); //14
                pasta.Add(""); //15
                pasta.Add(""); //16
                pasta.Add(""); //17
                pasta.Add(""); //18
                pasta.Add(""); //19
                pasta.Add(""); //20

            var r = new Random();
                var pastas = r.Next(pasta.Count);
                return pasta[pastas];
            }
            private static string Cookies()
            {
                var cookies = new List<string>();
                cookies.Add("https://i.imgur.com/obId3uA.png");
                cookies.Add("https://i.imgur.com/OCyTsTZ.png");
                cookies.Add("https://i.imgur.com/HybEZYH.png");
                cookies.Add("https://i.imgur.com/aCkF8Lf.png");
                cookies.Add("https://i.imgur.com/Gq83Qht.png");
                cookies.Add(""); //6
                cookies.Add(""); //7
                cookies.Add(""); //8
                cookies.Add(""); //9
                cookies.Add(""); //10
                cookies.Add(""); //11
                cookies.Add(""); //12
                cookies.Add(""); //13
                cookies.Add(""); //14
                cookies.Add(""); //15
                cookies.Add(""); //16
                cookies.Add(""); //17
                cookies.Add(""); //18
                cookies.Add(""); //19
                cookies.Add(""); //20

            var a = new Random();
                var cookie = a.Next(cookies.Count);
                return cookies[cookie];
            }
            private static string Pizza()
            {
                var pizza = new List<string>();
                pizza.Add(""); //1
                pizza.Add(""); //2
                pizza.Add(""); //3
                pizza.Add(""); //4
                pizza.Add(""); //5
                pizza.Add(""); //6
                pizza.Add(""); //7
                pizza.Add(""); //8
                pizza.Add(""); //9
                pizza.Add(""); //10
                pizza.Add(""); //11
                pizza.Add(""); //12
                pizza.Add(""); //13
                pizza.Add(""); //14
                pizza.Add(""); //15
                pizza.Add(""); //16
                pizza.Add(""); //17
                pizza.Add(""); //18
                pizza.Add(""); //19
                pizza.Add(""); //20

                var r = new Random();
                var pizzas = r.Next(pizza.Count);
                return pizza[pizzas];
            }
            private static string Salad()
            {
                var sala = new List<string>();
                sala.Add(""); //1
                sala.Add(""); //2
                sala.Add(""); //3
                sala.Add(""); //4
                sala.Add(""); //5
                sala.Add(""); //6
                sala.Add(""); //7
                sala.Add(""); //8
                sala.Add(""); //9
                sala.Add(""); //10
                sala.Add(""); //11
                sala.Add(""); //12
                sala.Add(""); //13
                sala.Add(""); //14
                sala.Add(""); //15
                sala.Add(""); //16
                sala.Add(""); //17
                sala.Add(""); //18
                sala.Add(""); //19
                sala.Add(""); //20

                var r = new Random();
                var salad = r.Next(sala.Count);
                return sala[salad];
            }
            private static string Sushi()
            {
                var sush = new List<string>();
                sush.Add(""); //1
                sush.Add(""); //2
                sush.Add(""); //3
                sush.Add(""); //4
                sush.Add(""); //5
                sush.Add(""); //6
                sush.Add(""); //7
                sush.Add(""); //8
                sush.Add(""); //9
                sush.Add(""); //10
                sush.Add(""); //11
                sush.Add(""); //12
                sush.Add(""); //13
                sush.Add(""); //14
                sush.Add(""); //15
                sush.Add(""); //16
                sush.Add(""); //17
                sush.Add(""); //18
                sush.Add(""); //19
                sush.Add(""); //20

                var r = new Random();
                var sushi = r.Next(sush.Count);
                return sush[sushi];
            }
            private static string Fruit()
            {
                var fruit = new List<string>();
                fruit.Add(""); //1
                fruit.Add(""); //2
                fruit.Add(""); //3
                fruit.Add(""); //4
                fruit.Add(""); //5
                fruit.Add(""); //6
                fruit.Add(""); //7
                fruit.Add(""); //8
                fruit.Add(""); //9
                fruit.Add(""); //10
                fruit.Add(""); //11
                fruit.Add(""); //12
                fruit.Add(""); //13
                fruit.Add(""); //14
                fruit.Add(""); //15
                fruit.Add(""); //16
                fruit.Add(""); //17
                fruit.Add(""); //18
                fruit.Add(""); //19
                fruit.Add(""); //20

                var r = new Random();
                var fruits = r.Next(fruit.Count);
                return fruit[fruits];
            }
            private static string Vegetables()
            {
                var veg = new List<string>();
                veg.Add(""); //1
                veg.Add(""); //2
                veg.Add(""); //3
                veg.Add(""); //4
                veg.Add(""); //5
                veg.Add(""); //6
                veg.Add(""); //7
                veg.Add(""); //8
                veg.Add(""); //9
                veg.Add(""); //10
                veg.Add(""); //11
                veg.Add(""); //12
                veg.Add(""); //13
                veg.Add(""); //14
                veg.Add(""); //15
                veg.Add(""); //16
                veg.Add(""); //17
                veg.Add(""); //18
                veg.Add(""); //19
                veg.Add(""); //20

                var r = new Random();
                var vegg = r.Next(veg.Count);
                return veg[vegg];
            }
            private static string CannedFood()
            {
                var can = new List<string>();
                can.Add(""); //1
                can.Add(""); //2
                can.Add(""); //3
                can.Add(""); //4
                can.Add(""); //5
                can.Add(""); //6
                can.Add(""); //7
                can.Add(""); //8
                can.Add(""); //9
                can.Add(""); //10
                can.Add(""); //11
                can.Add(""); //12
                can.Add(""); //13
                can.Add(""); //14
                can.Add(""); //15
                can.Add(""); //16
                can.Add(""); //17
                can.Add(""); //18
                can.Add(""); //19
                can.Add(""); //20

                var r = new Random();
                var cann = r.Next(can.Count);
                return can[cann];
            }
            private static string Tacos()
            {
                var tacos = new List<string>();
                tacos.Add(""); //1
                tacos.Add(""); //2
                tacos.Add(""); //3
                tacos.Add(""); //4
                tacos.Add(""); //5
                tacos.Add(""); //6
                tacos.Add(""); //7
                tacos.Add(""); //8
                tacos.Add(""); //9
                tacos.Add(""); //10
                tacos.Add(""); //11
                tacos.Add(""); //12
                tacos.Add(""); //13
                tacos.Add(""); //14
                tacos.Add(""); //15
                tacos.Add(""); //16
                tacos.Add(""); //17
                tacos.Add(""); //18
                tacos.Add(""); //19
                tacos.Add(""); //20

                var r = new Random();
                var tac = r.Next(tacos.Count);
                return tacos[tac];
            }
            private static string Pancakes()
            {
                var pan = new List<string>();
                pan.Add(""); //1
                pan.Add(""); //2
                pan.Add(""); //3
                pan.Add(""); //4
                pan.Add(""); //5
                pan.Add(""); //6
                pan.Add(""); //7
                pan.Add(""); //8
                pan.Add(""); //9
                pan.Add(""); //10
                pan.Add(""); //11
                pan.Add(""); //12
                pan.Add(""); //13
                pan.Add(""); //14
                pan.Add(""); //15
                pan.Add(""); //16
                pan.Add(""); //17
                pan.Add(""); //18
                pan.Add(""); //19
                pan.Add(""); //20

                var r = new Random();
                var cakes = r.Next(pan.Count);
                return pan[cakes];
            }
            private static string Coffee()
            {
                var coffee = new List<string>();
                coffee.Add(""); //1
                coffee.Add(""); //2
                coffee.Add(""); //3
                coffee.Add(""); //4
                coffee.Add(""); //5
                coffee.Add(""); //6
                coffee.Add(""); //7
                coffee.Add(""); //8
                coffee.Add(""); //9
                coffee.Add(""); //10
                coffee.Add(""); //11
                coffee.Add(""); //12
                coffee.Add(""); //13
                coffee.Add(""); //14
                coffee.Add(""); //15
                coffee.Add(""); //16
                coffee.Add(""); //17
                coffee.Add(""); //18
                coffee.Add(""); //19
                coffee.Add(""); //20

                var r = new Random();
                var coffees = r.Next(coffee.Count);
                return coffee[coffees];
            }
            private static string Soda()
            {
                var pop = new List<string>();
                pop.Add(""); //1
                pop.Add(""); //2
                pop.Add(""); //3
                pop.Add(""); //4
                pop.Add(""); //5
                pop.Add(""); //6
                pop.Add(""); //7
                pop.Add(""); //8
                pop.Add(""); //9
                pop.Add(""); //10
                pop.Add(""); //11
                pop.Add(""); //12
                pop.Add(""); //13
                pop.Add(""); //14
                pop.Add(""); //15
                pop.Add(""); //16
                pop.Add(""); //17
                pop.Add(""); //18
                pop.Add(""); //19
                pop.Add(""); //20

                var r = new Random();
                var pops = r.Next(pop.Count);
                return pop[pops];
            }
            private static string Tea()
            {
                var teas = new List<string>();
                teas.Add(""); //1
                teas.Add(""); //2
                teas.Add(""); //3
                teas.Add(""); //4
                teas.Add(""); //5
                teas.Add(""); //6
                teas.Add(""); //7
                teas.Add(""); //8
                teas.Add(""); //9
                teas.Add(""); //10
                teas.Add(""); //11
                teas.Add(""); //12
                teas.Add(""); //13
                teas.Add(""); //14
                teas.Add(""); //15
                teas.Add(""); //16
                teas.Add(""); //17
                teas.Add(""); //18
                teas.Add(""); //19
                teas.Add(""); //20

                var r = new Random();
                var teasl = r.Next(teas.Count);
                return teas[teasl];
            }
            private static string IceCream()
            {
                var icecream = new List<string>();
                icecream.Add(""); //1
                icecream.Add(""); //2
                icecream.Add(""); //3
                icecream.Add(""); //4
                icecream.Add(""); //5
                icecream.Add(""); //6
                icecream.Add(""); //7
                icecream.Add(""); //8
                icecream.Add(""); //9
                icecream.Add(""); //10
                icecream.Add(""); //11
                icecream.Add(""); //12
                icecream.Add(""); //13
                icecream.Add(""); //14
                icecream.Add(""); //15
                icecream.Add(""); //16
                icecream.Add(""); //17
                icecream.Add(""); //18
                icecream.Add(""); //19
                icecream.Add(""); //20

                var r = new Random();
                var cream = r.Next(icecream.Count);
                return icecream[cream];
            }
            private static string Sandwich()
            {
                var cookies = new List<string>();
                cookies.Add(""); //1
                cookies.Add(""); //2
                cookies.Add(""); //3
                cookies.Add(""); //4
                cookies.Add(""); //5
                cookies.Add(""); //6
                cookies.Add(""); //7
                cookies.Add(""); //8
                cookies.Add(""); //9
                cookies.Add(""); //10
                cookies.Add(""); //11
                cookies.Add(""); //12
                cookies.Add(""); //13
                cookies.Add(""); //14
                cookies.Add(""); //15
                cookies.Add(""); //16
                cookies.Add(""); //17
                cookies.Add(""); //18
                cookies.Add(""); //19
                cookies.Add(""); //20

                var r = new Random();
                var cookie = r.Next(cookies.Count);
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
