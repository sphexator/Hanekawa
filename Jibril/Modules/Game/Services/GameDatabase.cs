using Discord;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using Jibril.Data.Variables;

namespace Jibril.Modules.Game.Services
{
    public class GameDatabase
    {
        public static string DB = @"Data Source = data\database.sqlite;Version=3;Foreign Keys=ON;";

        public static List<String> GameCheckExistingUser(IUser user)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var result = new List<String>();
                var sql = $"SELECT * FROM shipgame WHERE user_id = '{user.Id}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var userId = (string)reader["user_id"];
                    result.Add(userId);
                }
                connection.Close();
                return result;
            }
        }

        public static void UpdateClass(IUser user, string shipClass)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"UPDATE exp SET shipclass = '{shipClass}' WHERE user_id = '{user.Id}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                connection.Close();
                return;
            }
        }

        public static void UpdateNPCStart(IUser user, int enemyid, int health, int enemyhealth)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"UPDATE shipgame SET enemyid = '{enemyid}', health = '{health}', enemyhealth = '{enemyhealth}', enemyDamageTaken = '0', combatstatus = '1' WHERE user_id = '{user.Id}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                connection.Close();
                return;
            }
        }

        public static void AddNPCStart(IUser user, int enemyid, int health, int enemyhealth)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"INSERT INTO shipgame (user_id, health, damagetaken, combatstatus, enemyid, enemyDamageTaken, enemyhealth, killAmount) VALUES ('{user.Id}', '{health}', '0', '1', '{enemyid}', '0', {enemyhealth}, 0)";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                connection.Close();
                return;
            }
        }

        public static void AddNPCDefault(IUser user, int health)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"INSERT INTO shipgame (user_id, health, damagetaken, combatstatus, enemyid, enemyDamageTaken, enemyhealth, killAmount) VALUES ('{user.Id}', '{health}', '0', '0', '0', '0', 0, 0)";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                connection.Close();
                return;
            }
        }

        public static List<GameStatus>GetUserGameStatus(IUser user)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var result = new List<GameStatus>();
                var sql = $"SELECT * FROM shipgame WHERE user_id = '{user.Id}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var UserID = (string)reader["user_id"];
                    var health = (int)reader["health"];
                    var damagetaken = (int)reader["damagetaken"];
                    var combatstatus = (int)reader["combatstatus"];
                    var enemyid = (int)reader["enemyid"];
                    var enemyDamageTaken = (int)reader["enemyDamageTaken"];
                    var enemyhealth = (int)reader["enemyhealth"];
                    var killAmount = (int)reader["killAmount"];

                    result.Add(new GameStatus
                    {
                        UserID = UserID,
                        Health = health,
                        Damagetaken = damagetaken,
                        Combatstatus = combatstatus,
                        Enemyid = enemyid,
                        EnemyDamageTaken = enemyDamageTaken,
                        Enemyhealth = enemyhealth,
                        KillAmount = killAmount
                    });
                }
                connection.Close();
                return result;
            }
        }

        public static List<EnemyId> Enemy(int enemyid)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var result = new List<EnemyId>();
                var sql = $"SELECT * FROM enemyIdentity WHERE id = '{enemyid}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var id = (int)reader["id"];
                    var enemyName = (string)reader["enemyName"];
                    var imageName = (string)reader["imageName"];
                    var health = (int)reader["health"];
                    var damage = (int)reader["damage"];
                    var enemyClass = (string)reader["enemyClass"];
                    var expGain = (int)reader["expGain"];
                    var currencyGain = (int)reader["currencyGain"];

                    result.Add(new EnemyId
                    {
                        Id = id,
                        EnemyName = enemyName,
                        ImagePath = imageName,
                        Health = health,
                        Damage = damage,
                        EnemyClass = enemyClass,
                        ExpGain = expGain,
                        CurrenyGain = currencyGain
                    });
                }
                connection.Close();
                return result;
            }
        }

        public static void EnemyDamageTaken(int damage, IUser user)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"UPDATE shipgame SET enemyDamageTaken = enemyDamageTaken + {damage} WHERE user_id = {user.Id}";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                connection.Close();
                return;
            }
        }

        public static void UserDamageTaken(int damage, IUser user)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"UPDATE shipgame SET damageTaken = damageTaken + {damage} WHERE user_id = {user.Id}";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                connection.Close();
                return;
            }
        }

        public static void FightOver(int exp, int currency, IUser user)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"UPDATE exp SET xp = xp + '{exp}', tokens = tokens + '{currency}' WHERE user_id = {user.Id}";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                connection.Close();
                return;
            }
        }

        public static void FinishedNPCFight(IUser user)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"UPDATE shipgame SET combatstatus = '0', killAmount = killAmount + '1', enemyDamageTaken = '0' WHERE user_id = '{user.Id}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                connection.Close();
                return;
            }
        }

        public static void GameOverNPC(IUser user)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"UPDATE shipgame SET combatstatus = '0', enemyDamageTaken = '0' WHERE user_id = '{ user.Id }'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                connection.Close();
                return;
            }
        }

        public static void Repair(IUser user)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"UPDATE shipgame SET damagetaken = '0' WHERE user_id = '{user.Id}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                connection.Close();
                return;
            }
        }

        public static void Repair(IUser user, int health)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"UPDATE shipgame SET health = '{health}' WHERE user_id = '{user.Id}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                connection.Close();
                return;
            }
        }

        public static void UpdateUserHealth(IUser user, int health)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"UPDATE shipgame SET health = '{health}' WHERE user_id = '{user.Id}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                connection.Close();
                return;
            }
        }

        public static List<Classes> GetClasses()
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var result = new List<Classes>();
                var sql = $"SELECT * FROM classes ORDER BY level ASC LIMIT 5";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var level = (int)reader["level"];
                    var classes = (string)reader["class"];

                    result.Add(new Classes
                    {
                        level = level,
                        shipClass = classes
                    });
                }
                connection.Close();
                return result;
            }
        }

        public static void ChangeShipClass(IUser user, string shipClass)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"UPDATE exp SET shipClass = '{shipClass}' WHERE user_id = '{user.Id}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                connection.Close();
                return;
            }
        }
    }
}