using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Jibril.Data.Variables;
using MySql.Data.MySqlClient;
using Jibril.Services.INC.Data;
using Jibril.Services.Level.Lists;

namespace Jibril.Services.INC.Database
{
    public class DatabaseHungerGame
    {
        private readonly string _database = DbInfo.DbNorm;
        private readonly MySqlConnection _dbConnection;
        private readonly string _password = DbInfo.password;
        private const bool Pooling = false;
        private readonly string _server = DbInfo.server;
        private readonly string _username = DbInfo.username;

        private DatabaseHungerGame(string table)
        {
            _table = table;
            var stringBuilder = new MySqlConnectionStringBuilder
            {
                Server = _server,
                UserID = _username,
                Password = _password,
                Database = _database,
                SslMode = MySqlSslMode.None,
                Pooling = Pooling
            };
            var connectionString = stringBuilder.ToString();
            _dbConnection = new MySqlConnection(connectionString);
            _dbConnection.Open();
        }

        private string _table { get; }

        private MySqlDataReader FireCommand(string query)
        {
            if (_dbConnection == null)
                return null;
            var command = new MySqlCommand(query, _dbConnection);
            var mySqlReader = command.ExecuteReader();
            return mySqlReader;
        }

        private void CloseConnection()
        {
            _dbConnection?.Close();
        }

        public static void EnterUser(IUser user)
        {
            var database = new DatabaseHungerGame("hanekawa");
            var str =
                $"INSERT INTO hungergame (userid) VALUES ('{user.Id}')";
            database.FireCommand(str);
            database.CloseConnection();
        }

        public static void Stagger(IUser user)
        {
            var database = new DatabaseHungerGame("hanekawa");
            var rand = new Random();
            var stamina = rand.Next(15, 20);
            var hunger = rand.Next(5, 10);
            var thirst = rand.Next(10, 20);
            var sleep = rand.Next(20, 30);
            var str = $"UPDATE hungergame SET stamina = stamina + '{stamina}', hunger = hunger - '{hunger}', sleep = sleep - '{sleep}', thirst = thirst - '{thirst}' WHERE userid = {user.Id}";
            database.FireCommand(str);
            database.CloseConnection();
        }

        public static void AddDamage(IUser user, int damage)
        {
            var database = new DatabaseHungerGame("hanekawa");
            var str = $"UPDATE hungergame SET damageTaken = damageTaken - '{damage}' WHERE userid = {user.Id}";
            database.FireCommand(str);
            database.CloseConnection();
        }

        public static void AddFood(ulong id, string item)
        {
            var database = new DatabaseHungerGame("hanekawa");
            var str = $"UPDATE hungergame SET totalfood = totalfood + 1, {item} = {item} + 1 WHERE userid = {id}";
            database.FireCommand(str);
            database.CloseConnection();
        }

        public static void AddDrink(ulong id, string item)
        {
            var database = new DatabaseHungerGame("hanekawa");
            var str = $"UPDATE hungergame SET totalfood = totaldrink + 1, {item} = {item} + 1 WHERE userid = {id}";
            database.FireCommand(str);
            database.CloseConnection();
        }

        public static void AddWeapon(ulong id, string item)
        {
            var database = new DatabaseHungerGame("hanekawa");
            var str = $"UPDATE hungergame SET totalweapons = totalweapons + 1, {item} = {item} + 1 WHERE userid = {id}";
            database.FireCommand(str);
            database.CloseConnection();
        }

        public static void AddWeapon(ulong id, string item, string ammotype, int ammo)
        {
            var database = new DatabaseHungerGame("hanekawa");
            var str = $"UPDATE hungergame SET totalweapons = totalweapons + 1, {item} = {item} + 1, {ammotype} = {ammotype} + {ammo} WHERE userid = {id}";
            database.FireCommand(str);
            database.CloseConnection();
        }

        public static void DieEvent(ulong id)
        {
            var database = new DatabaseHungerGame("hanekawa");
            var str = $"UPDATE hungergame SET status = '0' WHERE userid = {id}";
            database.FireCommand(str);
            database.CloseConnection();
        }

        public static IEnumerable<Player> GetPlayer(IUser user)
        {
            var result = new List<Player>();
            var database = new DatabaseHungerGame("hanekawa");
            const string str = "SELECT * FROM hungergame";
            var exec = database.FireCommand(str);
            while (exec.Read())
            {
                var id = (int) exec["id"];
                var userId = (ulong) exec["userid"];
                var health = (int) exec["health"];
                var stamina = (int) exec["stamina"];
                var damage = (int) exec["damage"];
                var hunger = (int) exec["hunger"];
                var thirst = (int) exec["thirst"];
                var sleep = (int) exec["sleep"];
                var status = (bool) exec["status"];
                var bleeding = (bool) exec["bleeding"];

                result.Add(new Player
                {
                    Id = id,
                    UserId = userId,
                    Health = health,
                    Stamina = stamina,
                    Damage = damage,
                    Hunger = hunger,
                    Thirst = thirst,
                    Sleep = sleep,
                    Status = status,
                    Bleeding = bleeding
                });
            }

            database.CloseConnection();
            return result;
        }

        public static IEnumerable<Weapons> GetWeapons()
        {
            var result = new List<Weapons>();
            var database = new DatabaseHungerGame("hanekawa");
            const string str = "SELECT * FROM hungergame";
            var exec = database.FireCommand(str);
            while (exec.Read())
            {
                var totalweapons = (int)exec["totalweapons"];
                var pistol = (int)exec["pistol"];
                var bullets = (int)exec["bullets"];
                var bow = (int)exec["bow"];
                var arrows = (int)exec["arrows"];
                var axe = (int)exec["axe"];
                var trap = (int)exec["trap"];

                result.Add(new Weapons
                {
                    TotalWeapons = totalweapons,
                    Pistol = pistol,
                    Bullets = bullets,
                    Bow = bow,
                    Arrows = arrows,
                    Axe = axe,
                    Trap = trap
                });
            }

            database.CloseConnection();
            return result;
        }

        public static IEnumerable<Consumables> GetConsumables()
        {
            var result = new List<Consumables>();
            var database = new DatabaseHungerGame("hanekawa");
            const string str = "SELECT * FROM hungergame";
            var exec = database.FireCommand(str);
            while (exec.Read())
            {
                var totalfood = (int)exec["totalfood"];
                var totaldrink = (int)exec["totaldrink"];
                var beans = (int)exec["beans"];
                var pasta = (int)exec["pasta"];
                var fish = (int)exec["fish"];
                var ramen = (int)exec["ramen"];
                var coke = (int)exec["coke"];
                var water = (int)exec["water"];
                var mountaindew = (int)exec["mountaindew"];
                var redbull = (int)exec["redbull"];
                var bandages = (int)exec["bandages"];

                result.Add(new Consumables
                {
                    TotalFood = totalfood,
                    TotalDrink = totaldrink,
                    Beans = beans,
                    Pasta = pasta,
                    Fish = fish,
                    Ramen = ramen,
                    Coke = coke,
                    Water = water,
                    MountainDew = mountaindew,
                    Redbull = redbull,
                    Bandages = bandages
                });
            }

            database.CloseConnection();
            return result;
        }
    }
}
