using System.Collections.Generic;
using Discord;
using Jibril.Data.Variables;
using MySql.Data.MySqlClient;

namespace Jibril.Modules.Game.Services
{
    public class GameDatabase
    {
        private readonly string _database = DbInfo.DbNorm;
        private readonly MySqlConnection _dbConnection;
        private readonly string _password = DbInfo.Password;
        private readonly bool _pooling = false;
        private readonly string _server = DbInfo.Server;
        private readonly string _username = DbInfo.Username;

        public GameDatabase(string table)
        {
            _table = table;
            var stringBuilder = new MySqlConnectionStringBuilder
            {
                Server = _server,
                UserID = _username,
                Password = _password,
                Database = _database,
                SslMode = MySqlSslMode.None,
                Pooling = _pooling
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
            if (_dbConnection != null)
                _dbConnection.Close();
        }


        public static List<string> GameCheckExistingUser(IUser user)
        {
            var result = new List<string>();
            var database = new GameDatabase("hanekawa");
            var str = string.Format("SELECT * FROM shipgame WHERE user_id = '{0}'", user.Id);
            var reader = database.FireCommand(str);
            while (reader.Read())
            {
                var userId = (string) reader["user_id"];

                result.Add(userId);
            }

            return result;
        }

        public static void UpdateClass(IUser user, string shipClass)
        {
            var database = new GameDatabase("hanekawa");
            var str = $"UPDATE exp SET shipclass = '{shipClass}' WHERE user_id = {user.Id}";
            var reader = database.FireCommand(str);
            database.CloseConnection();
        }

        public static void UpdateNPCStart(IUser user, int enemyid, int health, int enemyhealth)
        {
            var database = new GameDatabase("hanekawa");
            var str =
                $"UPDATE shipgame SET enemyid = '{enemyid}', health = '{health}', enemyhealth = '{enemyhealth}', enemyDamageTaken = '0', combatstatus = '1' WHERE user_id = '{user.Id}'";
            var reader = database.FireCommand(str);
            database.CloseConnection();
        }

        public static void AddNPCStart(IUser user, int enemyid, int health, int enemyhealth)
        {
            var database = new GameDatabase("hanekawa");
            var str =
                $"INSERT INTO shipgame (user_id, health, damagetaken, combatstatus, enemyid, enemyDamageTaken, enemyhealth, killAmount) VALUES ('{user.Id}', '{health}', '0', '1', '{enemyid}', '0', {enemyhealth}, 0)";
            var reader = database.FireCommand(str);
            database.CloseConnection();
        }

        public static void CreateGameDBEntry(IUser user)
        {
            var database = new GameDatabase("hanekawa");
            var str =
                $"INSERT INTO shipgame (user_id, health, damagetaken, combatstatus, enemyid, enemyDamageTaken, enemyhealth, killAmount) VALUES ('{user.Id}', '10', '0', '0', '0', '0', 9999, 0)";
            var reader = database.FireCommand(str);
            database.CloseConnection();
        }

        public static void AddNPCDefault(IUser user, int health)
        {
            var database = new GameDatabase("hanekawa");
            var str =
                $"INSERT INTO shipgame (user_id, health, damagetaken, combatstatus, enemyid, enemyDamageTaken, enemyhealth, killAmount) VALUES ('{user.Id}', '{health}', '0', '0', '0', '0', 0, 0)";
            var reader = database.FireCommand(str);
            database.CloseConnection();
        }

        public static List<GameStatus> GetUserGameStatus(IUser user)
        {
            var result = new List<GameStatus>();
            var database = new GameDatabase("hanekawa");
            var str = string.Format("SELECT * FROM shipgame WHERE user_id = '{0}'", user.Id);
            var reader = database.FireCommand(str);
            while (reader.Read())
            {
                var UserID = (string) reader["user_id"];
                var health = (int) reader["health"];
                var damagetaken = (int) reader["damagetaken"];
                var combatstatus = (int) reader["combatstatus"];
                var enemyid = (int) reader["enemyid"];
                var enemyDamageTaken = (int) reader["enemyDamageTaken"];
                var enemyhealth = (int) reader["enemyhealth"];
                var killAmount = (int) reader["killAmount"];

                result.Add(new GameStatus
                {
                    UserId = UserID,
                    Health = health,
                    Damagetaken = damagetaken,
                    Combatstatus = combatstatus,
                    Enemyid = enemyid,
                    EnemyDamageTaken = enemyDamageTaken,
                    Enemyhealth = enemyhealth,
                    KillAmount = killAmount
                });
            }

            database.CloseConnection();
            return result;
        }

        public static List<EnemyId> Enemy(int enemyid)
        {
            var result = new List<EnemyId>();
            var database = new GameDatabase("hanekawa");
            var str = string.Format("SELECT * FROM enemyIdentity WHERE id = '{0}'", enemyid);
            var reader = database.FireCommand(str);

            while (reader.Read())
            {
                var id = (int) reader["id"];
                var enemyName = (string) reader["enemyName"];
                var imageName = (string) reader["imageName"];
                var health = (int) reader["health"];
                var damage = (int) reader["damage"];
                var enemyClass = (string) reader["enemyClass"];
                var expGain = (int) reader["expGain"];
                var currencyGain = (int) reader["currencyGain"];

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

            database.CloseConnection();
            return result;
        }

        public static void EnemyDamageTaken(int damage, IUser user)
        {
            var database = new GameDatabase("hanekawa");
            var str = $"UPDATE shipgame SET enemyDamageTaken = enemyDamageTaken + {damage} WHERE user_id = {user.Id}";
            var reader = database.FireCommand(str);
            database.CloseConnection();
        }

        public static void UserDamageTaken(int damage, IUser user)
        {
            var database = new GameDatabase("hanekawa");
            var str = $"UPDATE shipgame SET damageTaken = damageTaken + {damage} WHERE user_id = {user.Id}";
            var reader = database.FireCommand(str);
            database.CloseConnection();
        }

        public static void FightOver(int exp, int currency, IUser user)
        {
            var database = new GameDatabase("hanekawa");
            var str = $"UPDATE exp SET xp = xp + '{exp}', tokens = tokens + '{currency}' WHERE user_id = {user.Id}";
            var reader = database.FireCommand(str);
            database.CloseConnection();
        }

        public static void FinishedNPCFight(IUser user)
        {
            var database = new GameDatabase("hanekawa");
            var str =
                $"UPDATE shipgame SET combatstatus = '0', killAmount = killAmount + '1', enemyDamageTaken = '0' WHERE user_id = '{user.Id}'";
            var reader = database.FireCommand(str);
            database.CloseConnection();
        }

        public static void GameOverNPC(IUser user)
        {
            var database = new GameDatabase("hanekawa");
            var str = $"UPDATE shipgame SET combatstatus = '0', enemyDamageTaken = '0' WHERE user_id = '{user.Id}'";
            var reader = database.FireCommand(str);
            database.CloseConnection();
        }

        public static void Repair(IUser user)
        {
            var database = new GameDatabase("hanekawa");
            var str = $"UPDATE shipgame SET damagetaken = '0' WHERE user_id = '{user.Id}'";
            var reader = database.FireCommand(str);
            database.CloseConnection();
        }

        public static void UpdateUserHealth(IUser user, int health)
        {
            var database = new GameDatabase("hanekawa");
            var str = $"UPDATE shipgame SET health = '{health}' WHERE user_id = '{user.Id}'";

            var reader = database.FireCommand(str);
            database.CloseConnection();
        }

        public static IEnumerable<Classes> GetClasses()
        {
            var result = new List<Classes>();
            var database = new GameDatabase("hanekawa");
            var str = $"SELECT * FROM classes ORDER BY level ASC LIMIT 5";
            var reader = database.FireCommand(str);
            while (reader.Read())
            {
                var level = (int) reader["level"];
                var classes = (string) reader["class"];

                result.Add(new Classes
                {
                    Level = level,
                    Class = classes
                });
            }

            database.CloseConnection();
            return result;
        }

        public static void ChangeShipClass(IUser user, string shipClass)
        {
            var database = new GameDatabase("hanekawa");
            var str = $"UPDATE exp SET shipClass = '{shipClass}' WHERE user_id = '{user.Id}'";
            var reader = database.FireCommand(str);
            database.CloseConnection();
        }
    }
}