using System;
using Microsoft.Data.Sqlite;
using System.IO;

namespace HenkChat
{
    class Database
    {
        SqliteConnection Connection;
        public void Open(string ServerFolder)
        {
            if (!File.Exists(Path.Combine(ServerFolder, "Data.sqlite")))
            {
                Connection = new SqliteConnection($"Data Source={Path.Combine(ServerFolder, "Data.sqlite")};");
                Connection.Open();

                using (SqliteCommand Command = new SqliteCommand("CREATE TABLE Messages (ID INTEGER PRIMARY KEY, Message VARBINARY)", Connection)) Command.ExecuteNonQuery();
            }
            else
            {
                Connection = new SqliteConnection($"Data Source={Path.Combine(ServerFolder, "Data.sqlite")};");
                Connection.Open();
            }
        }

        public void Save(byte[] Data)
        {
            using (SqliteCommand Command = new SqliteCommand("INSERT INTO Messages (Message) VALUES (@Data)", Connection))
            {
                Command.Parameters.AddWithValue("@Data", Data);
                Command.ExecuteNonQueryAsync();
            }
        }

        public byte[] Read(int ID)
        {
            using (SqliteCommand Command = new SqliteCommand("SELECT Message FROM Messages WHERE ID = @ID", Connection))
            {
                Command.Parameters.AddWithValue("@ID", ID);
                SqliteDataReader Reader = Command.ExecuteReader();
                if (Reader.Read()) return Reader["Message"] as byte[];
                else return null;
            }
        }

        public void Clear() => new SqliteCommand("DELETE FROM messages", Connection).ExecuteNonQuery();

        public Int64 GetMessagesCount() { using (SqliteCommand Command = new SqliteCommand("SELECT COUNT(*) FROM Messages", Connection)) { return (Int64)Command.ExecuteScalar(); } }
    }
}
