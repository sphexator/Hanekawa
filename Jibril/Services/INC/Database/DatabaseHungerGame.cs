﻿using System;
using System.Collections.Generic;
using Discord;
using Jibril.Data.Variables;
using Jibril.Extensions;
using Jibril.Services.INC.Data;
using MySql.Data.MySqlClient;

namespace Jibril.Services.INC.Database
{
    public class DatabaseHungerGame
    {
        private const bool Pooling = false;
        private readonly string _database = DbInfo.DbNorm;
        private readonly MySqlConnection _dbConnection;
        private readonly string _password = DbInfo.Password;
        private readonly string _server = DbInfo.Server;
        private readonly string _username = DbInfo.Username;

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
            var name = user.Username.RemoveSpecialCharacters();
            var database = new DatabaseHungerGame("hanekawa");
            var str =
                $"INSERT INTO hungergame (userid, name) VALUES ('{user.Id}', '{name}')";
            database.FireCommand(str);
            database.CloseConnection();
        }

        public static void GameSignUpStart()
        {
            var database = new DatabaseHungerGame("hanekawa");
            const string str = "UPDATE hungergameconfig SET signupstage = 1 WHERE guild = '339370914724446208'";
            database.FireCommand(str);
            database.CloseConnection();
        }

        public static void GameStart()
        {
            var database = new DatabaseHungerGame("hanekawa");
            const string str =
                "UPDATE hungergameconfig SET live = 1, round = 1, signupstage = 0 WHERE guild = '339370914724446208'";
            database.FireCommand(str);
            database.CloseConnection();
        }

        public static void GameEnd()
        {
            var database = new DatabaseHungerGame("hanekawa");
            const string str = "UPDATE hungergameconfig SET live = 0, round = 0 WHERE guild = '339370914724446208'";
            database.FireCommand(str);
            database.CloseConnection();
        }

        public static void GameRoundIncrease()
        {
            var database = new DatabaseHungerGame("hanekawa");
            const string str = "UPDATE hungergameconfig SET round = round + 1 WHERE guild = '339370914724446208'";
            database.FireCommand(str);
            database.CloseConnection();
        }

        public static List<Config> GetConfig()
        {
            var result = new List<Config>();
            var database = new DatabaseHungerGame("hanekawa");
            const string str = "SELECT * FROM hungergameconfig WHERE guildid = '339370914724446208'";
            var exec = database.FireCommand(str);
            while (exec.Read())
            {
                var guildid = (ulong) exec["guild"];
                var live = (bool) exec["live"];
                var round = (int) exec["round"];
                var signnupDuration = (DateTime) exec["signupDuration"];
                var signUpStage = (bool) exec["signupstage"];

                result.Add(new Config
                {
                    GuildId = guildid,
                    Live = live,
                    Round = round,
                    SignupDuration = signnupDuration,
                    SignUpStage = signUpStage
                });
            }

            database.CloseConnection();
            return result;
        }

        public static List<Player> GetUsers()
        {
            var result = new List<Player>();
            var database = new DatabaseHungerGame("hanekawa");
            const string str = "SELECT * FROM hungergame WHERE status = '1'";
            var exec = database.FireCommand(str);
            while (exec.Read())
            {
                var id = (int) exec["id"];
                var userId = (ulong) exec["userid"];
                var name = (string) exec["name"];
                var health = (int) exec["health"];
                var stamina = (int) exec["stamina"];
                var damage = (int) exec["damageTaken"];
                var hunger = (int) exec["hunger"];
                var thirst = (int) exec["thirst"];
                var sleep = (int) exec["sleep"];
                var status1 = (int) exec["status"];
                var bleeding1 = (int) exec["bleeding"];

                var status = false;
                var bleeding = false;

                if (status1 == 1) status = true;
                if (bleeding1 == 1) bleeding = true;

                result.Add(new Player
                {
                    Id = id,
                    UserId = userId,
                    Name = name,
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

        public static List<Player> CheckExistingUser(IUser user)
        {
            var result = new List<Player>();
            var database = new DatabaseHungerGame("hanekawa");
            var str = $"SELECT * FROM hungergame WHERE userid = '{user.Id}'";
            var exec = database.FireCommand(str);
            while (exec.Read())
            {
                var id = (int) exec["id"];
                var userId = (ulong) exec["userid"];
                var health = (int) exec["health"];
                var stamina = (int) exec["stamina"];
                var damage = (int) exec["damageTaken"];
                var hunger = (int) exec["hunger"];
                var thirst = (int) exec["thirst"];
                var sleep = (int) exec["sleep"];
                var status1 = (int) exec["status"];
                var bleeding1 = (int) exec["bleeding"];

                var status = false;
                var bleeding = false;

                if (status1 == 1) status = true;
                if (bleeding1 == 1) bleeding = true;

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

        public static void Stagger(ulong user)
        {
            var database = new DatabaseHungerGame("hanekawa");
            var rand = new Random();
            var stamina = rand.Next(15, 20);
            var hunger = rand.Next(5, 10);
            var thirst = rand.Next(10, 20);
            var sleep = rand.Next(20, 30);
            var str =
                $"UPDATE hungergame SET stamina = stamina + '{stamina}', hunger = hunger + '{hunger}', sleep = sleep + '{sleep}', thirst = thirst + '{thirst}' WHERE userid = {user}";
            database.FireCommand(str);
            database.CloseConnection();
        }

        public static void Sleep(ulong userid)
        {
            var database = new DatabaseHungerGame("hanekawa");
            var str = $"UPDATE hungergame SET sleep = 0 WHERE userid = {userid}";
            database.FireCommand(str);
            database.CloseConnection();
        }

        public static void EatFood(ulong userid)
        {
            var database = new DatabaseHungerGame("hanekawa");
            var str = $"UPDATE hungergame SET hunger = 0 WHERE userid = {userid}";
            database.FireCommand(str);
            database.CloseConnection();
        }

        public static void EatSpecialFood(ulong userid, int buff)
        {
            var database = new DatabaseHungerGame("hanekawa");
            var str = $"UPDATE hungergame SET hunger = 0, stamina = stamina + '{buff}' WHERE userid = {userid}";
            database.FireCommand(str);
            database.CloseConnection();
        }

        public static void DrinkWater(ulong userid)
        {
            var database = new DatabaseHungerGame("hanekawa");
            var str = $"UPDATE hungergame SET thirst = 0 WHERE userid = {userid}";
            database.FireCommand(str);
            database.CloseConnection();
        }

        public static void AddDamage(IUser user, int damage)
        {
            var database = new DatabaseHungerGame("hanekawa");
            var str = $"UPDATE hungergame SET damageTaken = damageTaken + '{damage}' WHERE userid = {user.Id}";
            database.FireCommand(str);
            database.CloseConnection();
        }

        public static void AddDamage(ulong user, int damage)
        {
            var database = new DatabaseHungerGame("hanekawa");
            var str = $"UPDATE hungergame SET damageTaken = damageTaken + '{damage}' WHERE userid = {user}";
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

        public static void ConsumeFood(ulong id, string item)
        {
            var database = new DatabaseHungerGame("hanekawa");
            var str = $"UPDATE hungergame SET totalfood = totalfood - 1, {item} = {item} - 1 WHERE userid = {id}";
            database.FireCommand(str);
            database.CloseConnection();
        }

        public static void ConsumeDrink(ulong id, string item)
        {
            var database = new DatabaseHungerGame("hanekawa");
            var str = $"UPDATE hungergame SET totalfood = totaldrink - 1, {item} = {item} - 1 WHERE userid = {id}";
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
            var str =
                $"UPDATE hungergame SET totalweapons = totalweapons + 1, {item} = {item} + 1, {ammotype} = {ammotype} + {ammo} WHERE userid = {id}";
            database.FireCommand(str);
            database.CloseConnection();
        }

        public static void UseAmmo(ulong id, string item, string ammotype, int ammo)
        {
            var database = new DatabaseHungerGame("hanekawa");
            var str = $"UPDATE hungergame SET {ammotype} = {ammotype} - {ammo} WHERE userid = {id}";
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

        public static List<Profile> GetProfilEnumerable()
        {
            var result = new List<Profile>();
            var database = new DatabaseHungerGame("hanekawa");
            const string str = "SELECT * FROM hungergame";
            var exec = database.FireCommand(str);
            while (exec.Read())
            {
                var status = false;
                var bleeding = false;

                var id = (int) exec["id"];
                var userId = (ulong) exec["userid"];
                var name = (string) exec["name"];
                var health = (int) exec["health"];
                var stamina = (int) exec["stamina"];
                var damage = (int) exec["damageTaken"];
                var hunger = (int) exec["hunger"];
                var thirst = (int) exec["thirst"];
                var sleep = (int) exec["sleep"];
                var status1 = (int) exec["status"];
                var bleeding1 = (int) exec["bleeding"];
                var totalfood = (int) exec["totalfood"];
                var totaldrink = (int) exec["totaldrink"];
                var beans = (int) exec["beans"];
                var pasta = (int) exec["pasta"];
                var fish = (int) exec["fish"];
                var ramen = (int) exec["ramen"];
                var coke = (int) exec["coke"];
                var water = (int) exec["water"];
                var mountaindew = (int) exec["mountaindew"];
                var redbull = (int) exec["redbull"];
                var bandages = (int) exec["bandages"];
                var totalweapons = (int) exec["totalweapons"];
                var pistol = (int) exec["pistol"];
                var bullets = (int) exec["bullets"];
                var bow = (int) exec["bow"];
                var arrows = (int) exec["arrows"];
                var axe = (int) exec["axe"];
                var trap = (int) exec["trap"];

                if (status1 == 1) status = true;
                if (bleeding1 == 1) bleeding = true;

                result.Add(new Profile
                {
                    Player = new Player
                    {
                        Id = id,
                        UserId = userId,
                        Name = name,
                        Health = health,
                        Stamina = stamina,
                        Damage = damage,
                        Hunger = hunger,
                        Thirst = thirst,
                        Sleep = sleep,
                        Status = status,
                        Bleeding = bleeding
                    },
                    Weapons = new Weapons
                    {
                        TotalWeapons = totalweapons,
                        Pistol = pistol,
                        Bullets = bullets,
                        Bow = bow,
                        Arrows = arrows,
                        Axe = axe,
                        Trap = trap
                    },
                    Consumables = new Consumables
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
                    }
                });
            }

            database.CloseConnection();
            return result;
        }
    }
}