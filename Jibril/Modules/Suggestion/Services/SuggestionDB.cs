using Discord;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace Jibril.Modules.Suggestion.Services
{
    public class SuggestionDB
    {
        public static string DB = @"Data Source = Data/database.sqlite;Version=3;Foreign Keys=ON;";
        public static void AddSuggestion(IUser user, DateTime now)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"INSERT INTO suggestion (user_id, date) VALUES ('{user.Id}', '{now}')";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
                connection.Close();
                return;
            }
        }

        public static List<int> GetSuggestionID(DateTime time)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();

                var result = new List<int>();
                var sql = $"SELECT * FROM suggestion WHERE date = '{time}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var suggestNr = (int)reader["id"];
                    result.Add(suggestNr);
                }

                connection.Close();
                return result;
            }
        }

        public static void UpdateSuggestion(string msgid, int id)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"UPDATE suggestion SET msgid = '{msgid}' WHERE id = '{id}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
                connection.Close();
                return;
            }
        }

        public static void RespondSuggestion(uint casenr, string response, IUser user)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();
                var sql = $"UPDATE suggestion SET responduser = '{user.Id}', response = '{response}' WHERE id = '{casenr}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
                connection.Close();
                return;
            }
        }

        public static List<String> SuggestionMessage(uint casenr)
        {
            using (SQLiteConnection connection = new SQLiteConnection(DB))
            {
                connection.Open();

                var result = new List<String>();
                var sql = $"SELECT * FROM suggestion WHERE id = '{casenr}'";
                SQLiteCommand command = new SQLiteCommand(sql, connection);
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var msgid = (string)reader["msgid"];

                    result.Add(msgid);
                }

                connection.Close();
                return result;
            }
        }
    }
}
