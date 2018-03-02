using System;
using System.Collections.Generic;
using Discord;
using Jibril.Data.Variables;
using Jibril.Modules.Gambling.Lists;
using Jibril.Services.Level.Lists;
using MySql.Data.MySqlClient;

namespace Jibril.Modules.Gambling.Services
{
    public class GambleDB
    {
        private readonly string database = DbInfo.DbNorm;
        private readonly MySqlConnection dbConnection;
        private readonly string password = DbInfo.Password;
        private readonly bool POOLING = false;
        private readonly string server = DbInfo.Server;
        private readonly string username = DbInfo.Username;

        public GambleDB(string table)
        {
            _table = table;
            var stringBuilder = new MySqlConnectionStringBuilder
            {
                Server = server,
                UserID = username,
                Password = password,
                Database = database,
                SslMode = MySqlSslMode.None,
                Pooling = POOLING
            };
            var connectionString = stringBuilder.ToString();
            dbConnection = new MySqlConnection(connectionString);
            dbConnection.Open();
        }

        private string _table { get; }

        public MySqlDataReader FireCommand(string query)
        {
            if (dbConnection == null)
                return null;
            var command = new MySqlCommand(query, dbConnection);
            var mySqlReader = command.ExecuteReader();
            return mySqlReader;
        }

        public void CloseConnection()
        {
            if (dbConnection != null)
                dbConnection.Close();
        }

        public static void AddRep(IUser user)
        {
            var database = new GambleDB("hanekawa");
            var str = $"UPDATE exp SET rep = rep + 1 WHERE user_id = {user.Id}";
            database.FireCommand(str);
            database.CloseConnection();
        }

        public static void AddEventCredit(IUser user, int credit)
        {
            var database = new GambleDB("hanekawa");
            var str = $"UPDATE exp SET event_tokens = event_tokens + '{credit}' WHERE user_id = '{user.Id}'";
            database.FireCommand(str);
            database.CloseConnection();
        }

        public static void AddCredit(IUser user, int credit)
        {
            var database = new GambleDB("hanekawa");
            var str = $"UPDATE exp SET tokens = tokens + '{credit}' WHERE user_id = '{user.Id}'";
            database.FireCommand(str);
            database.CloseConnection();
        }

        public static void RemoveCredit(IUser user, int credit)
        {
            var database = new GambleDB("hanekawa");
            var str = $"UPDATE exp SET tokens = tokens - '{credit}' WHERE user_id = '{user.Id}'";
            var reader = database.FireCommand(str);
            database.CloseConnection();
        }

        public static void RemoveEventTokens(IUser user, int price)
        {
            var database = new GambleDB("hanekawa");
            var str = $"UPDATE exp SET event_tokens = event_tokens - '{price}' WHERE user_id = '{user.Id}'";
            var reader = database.FireCommand(str);
            database.CloseConnection();
        }

        public static List<UserData> GetLeaderBoard()
        {
            var database = new GambleDB("hanekawa");
            var result = new List<UserData>();
            var str = "SELECT * FROM exp ORDER BY tokens DESC LIMIT 10";
            var reader = database.FireCommand(str);

            while (reader.Read())
            {
                var userId = (string) reader["user_id"];
                var userName = (string) reader["username"];
                var currentTokens = (uint) reader["tokens"];
                var event_tokens = (uint) reader["event_tokens"];
                var level = (int) reader["level"];
                var exp = (int) reader["xp"];
                var totalExp = (int) reader["total_xp"];
                var daily = (DateTime) reader["daily"];
                var cooldown = (DateTime) reader["cooldown"];
                var voice_timer = (DateTime) reader["voice_timer"];
                var fleetName = (string) reader["fleetName"];
                var shipClass = (string) reader["shipClass"];
                var profilepic = (string) reader["shipclass"];
                var gameCD = (DateTime) reader["game_cooldown"];
                var gambleCD = (DateTime) reader["gambling_cooldown"];
                var hasrole = (string) reader["hasrole"];

                result.Add(new UserData
                {
                    UserId = userId,
                    Username = userName,
                    Tokens = currentTokens,
                    Event_tokens = event_tokens,
                    Level = level,
                    Xp = exp,
                    Total_xp = totalExp,
                    Daily = daily,
                    Cooldown = cooldown,
                    Voice_timer = voice_timer,
                    FleetName = fleetName,
                    ShipClass = shipClass,
                    Profilepic = profilepic,
                    GameCD = gameCD,
                    BetCD = gambleCD,
                    Hasrole = hasrole
                });
            }

            database.CloseConnection();
            return result;
        }

        // Shop items

        public static List<ShopList> Shoplist()
        {
            var database = new GambleDB("hanekawa");
            var result = new List<ShopList>();
            var str = "SELECT * FROM shop";
            var reader = database.FireCommand(str);

            while (reader.Read())
            {
                var id = (int) reader["id"];
                var Item = (string) reader["item"];
                var price = (int) reader["price"];

                result.Add(new ShopList
                {
                    Id = id,
                    Item = Item,
                    Price = price
                });
            }

            return result;
        }

        public static List<EventShopList> EventShopList()
        {
            var database = new GambleDB("hanekawa");
            var result = new List<EventShopList>();
            var str = "SELECT * FROM eventshop";
            var reader = database.FireCommand(str);

            while (reader.Read())
            {
                var id = (int) reader["id"];
                var item = (string) reader["item"];
                var price = (int) reader["price"];
                var stock = (int) reader["stock"];

                result.Add(new EventShopList
                {
                    Id = id,
                    Item = item,
                    Price = price,
                    Stock = stock
                });
            }

            database.CloseConnection();
            return result;
        }


        public static void BuyItem(IUser user, string itemName)
        {
            var database = new GambleDB("hanekawa");
            var str = $"UPDATE inventory SET {itemName} = {itemName} + '1' WHERE user_ID = '{user.Id}'";
            var tableName = database.FireCommand(str);
            database.CloseConnection();
        }

        public static void ChangeShopStockAmount()
        {
            var database = new GambleDB("hanekawa");
            var str = $"UPDATE eventshop SET stock = stock - '1'";
            var tableName = database.FireCommand(str);
            database.CloseConnection();
        }

        //Change Role Stuff
        public static List<string> CheckRoleStatus(IUser user)
        {
            var result = new List<string>();
            var database = new GambleDB("hanekawa");
            var str = $"SELECT hasrole FROM exp WHERE user_id = {user.Id}";
            var tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                var roleStatus = (string) tableName["hasrole"];
                result.Add(roleStatus);
            }

            database.CloseConnection();
            return result;
        }

        public static void UpdateRoleStatus(IUser user)
        {
            var database = new GambleDB("hanekawa");
            var str = $"UPDATE exp SET hasrole = 'yes' WHERE user_id = '{user.Id}'";
            var tableName = database.FireCommand(str);
            database.CloseConnection();
        }

        // Use items
        public static void UseItem(IUser user, string itemName)
        {
            var database = new GambleDB("hanekawa");
            var str = $"UPDATE inventory SET {itemName} = {itemName} - '1' WHERE user_ID = '{user.Id}'";
            var tableName = database.FireCommand(str);
            database.CloseConnection();
        }

        // Inventory stuff
        public static void CreateInventory(IUser user)
        {
            var database = new GambleDB("hanekawa");
            var str = $"INSERT INTO inventory (user_id) VALUES ('{user.Id}')";
            var tableName = database.FireCommand(str);
            database.CloseConnection();
        }

        public static List<InventoryList> Inventory(IUser user)
        {
            var result = new List<InventoryList>();
            var database = new GambleDB("hanekawa");
            var str = $"SELECT * FROM inventory WHERE user_id = '{user.Id}'";
            var reader = database.FireCommand(str);

            while (reader.Read())
            {
                var kit = (int) reader["RepairKit"];
                var dmg = (int) reader["DamageBoost"];
                var shield = (int) reader["Shield"];
                var customRole = (int) reader["CustomRole"];

                result.Add(new InventoryList
                {
                    Repairkit = kit,
                    Dmgboost = dmg,
                    Shield = shield,
                    CustomRole = customRole
                });
            }

            database.CloseConnection();
            return result;
        }
    }
}