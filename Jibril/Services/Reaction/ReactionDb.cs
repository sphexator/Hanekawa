using Jibril.Services.Reaction.List;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace Jibril.Services.Reaction
{
    public class ReactionDb
    {
        public static string DB = @"Data Source = Data/database.sqlite;Version=3;Foreign Keys=ON;";

        public static void InsertReactionMessage(string msgid, string channelid, int counter)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"INSERT INTO reaction (msgid, chid, counter) VALUES ('{msgid}', '{channelid}', '{counter}')";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
                connection.Close();
                return;
            }
        }

        public static List<ReactionList> ReactionData(string msgid)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var result = new List<ReactionList>();
                var sql = $"SELECT * FROM reaction WHERE msgid = {msgid}";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var messageid = (string)reader["msgid"];
                    var chid = (string)reader["chid"];
                    var counter = (int)reader["counter"];
                    var sent = (string)reader["sent"];

                    result.Add(new ReactionList
                    {
                        Msgid = msgid,
                        Chid = chid,
                        Counter = counter,
                        Sent = sent
                    });
                }
                connection.Close();
                return result;
            }
        }

        public static void AddReaction(string msgid)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"UPDATE reaction SET counter = counter + '1' WHERE msgid = '{msgid}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
                connection.Close();
                return;
            }
        }
        public static void RemoveReaction(string msgid)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"UPDATE reaction SET counter = counter - '1' WHERE msgid = '{msgid}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
                connection.Close();
                return;
            }
        }
        public static void ReactionMsgPosted(string msgid)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"UPDATE reaction SET sent = 'yes' WHERE msgid = '{msgid}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
                connection.Close();
                return;
            }
        }
    }
}
