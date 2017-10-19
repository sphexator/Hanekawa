using Discord;
using Jibril.Modules.Gambling.Lists;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace Jibril.Modules.Gambling.Services
{
    public class GambleDB
    {
        public static string DB = @"Data Source = data\database.db;Version=3;Foreign Keys=ON;";
        public static void AddCredit(IUser user, int credit)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"UPDATE exp SET tokens = tokens + '{credit}' WHERE user_id = '{user.Id}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                connection.Close();
                return;
            }
        }

        public static void RemoveCredit(IUser user, int credit)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"UPDATE exp SET tokens = tokens - '{credit}' WHERE user_id = '{user.Id}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                connection.Close();
                return;
            }
        }

        // Shop items

        public static List<ShopList> Shoplist()
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();

                var result = new List<ShopList>();
                var sql = "SELECT * FROM shop";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var id = (int)reader["id"];
                    var Item = (string)reader["item"];
                    var price = (int)reader["price"];

                    result.Add(new ShopList
                    {
                        Id = id,
                        Item = Item,
                        Price = price
                    });
                }

                connection.Close();
                return result;
            }
        }

        public static List<EventShopList> EventShopList()
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();

                var result = new List<EventShopList>();
                var sql = "SELECT * FROM eventshop";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var id = (int)reader["id"];
                    var item = (string)reader["item"];
                    var price = (int)reader["price"];
                    var stock = (int)reader["stock"];

                    result.Add(new EventShopList
                    {
                        Id = id,
                        Item = item,
                        Price = price,
                        Stock = stock
                    });
                }
                connection.Close();
                return result;
            }
        }

        public static void BuyItem(IUser user, string itemName)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"UPDATE inventory SET {itemName} = {itemName} + '1' WHERE user_ID = '{user.Id}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                connection.Close();
                return;
            }
        }

        public static void RemoveEventTokens(IUser user, int price)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"UPDATE exp SET event_tokens = event_tokens - '{price}' WHERE user_id = '{user.Id}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                connection.Close();
                return;
            }
        }

        public static void ChangeShopStockAmount()
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"UPDATE eventshop SET stock = stock - '1'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                connection.Close();
                return;
            }
        }

        public static List<String> CheckRoleStatus(IUser user)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();

                var result = new List<String>();
                var sql = $"SELECT hasrole FROM exp WHERE user_id = {user.Id}";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var roleStatus = (string)reader["hasrole"];
                    result.Add(roleStatus);
                }

                connection.Close();
                return result;
            }
        }

        public static void UpdateRoleStatus(IUser user)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"UPDATE exp SET hasrole = 'yes' WHERE user_id = '{user.Id}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                connection.Close();
                return;
            }
        }

        public static void UseItem(IUser user, string itemName)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"UPDATE inventory SET {itemName} = {itemName} - '1' WHERE user_ID = '{user.Id}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                connection.Close();
                return;
            }
        }

        public static void CreateInventory(IUser user)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"INSERT INTO inventory (user_id) VALUES ('{user.Id}')";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                connection.Close();
                return;
            }
        }

        public static List<InventoryList> Inventory(IUser user)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();

                var result = new List<InventoryList>();
                var sql = $"SELECT * FROM inventory WHERE user_id = '{user.Id}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var kit = (int)reader["RepairKit"];
                    var dmg = (int)reader["DamageBoost"];
                    var shield = (int)reader["Shield"];
                    var customRole = (int)reader["CustomRole"];

                    result.Add(new InventoryList
                    {
                        Repairkit = kit,
                        Dmgboost = dmg,
                        Shield = shield,
                        CustomRole = customRole
                    });
                }

                connection.Close();
                return result;
            }
        }
    }
}
