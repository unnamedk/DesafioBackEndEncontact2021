using Microsoft.Data.Sqlite;
using System;
using TesteBackendEnContact.Database;

namespace UnitTests
{
    public class DatabaseFixture : IDisposable
    {
        public DatabaseConfig Config;
        public SqliteConnection Sql;

        public DatabaseFixture()
        {
            Config = new DatabaseConfig
            {
                ConnectionString = "Data Source=DatabaseFixture;Mode=Memory;Cache=Shared"
            };

            Sql = new SqliteConnection(Config.ConnectionString);
            Sql.Open();

            var query = new SqliteCommand
            {
                CommandText = "CREATE TABLE IF NOT EXISTS ContactBook ( Id INTEGER PRIMARY KEY, Name VARCHAR(50) NOT NULL)",
                Connection = Sql
            };
            query.ExecuteNonQuery();

            query.CommandText = @"
CREATE TABLE IF NOT EXISTS Contact (
Id INTEGER PRIMARY KEY,
ContactBookId INTEGER,
CompanyId INTEGER,
Name VARCHAR(50),
Phone VARCHAR(20),
Email VARCHAR(50),
Address VARCHAR(100) )";
            query.ExecuteNonQuery();

            query.CommandText = @"
CREATE TABLE IF NOT EXISTS Company (
Id INTEGER PRIMARY KEY,
ContactBookId INTEGER,
Name VARCHAR(50) )";
            query.ExecuteNonQuery();
        }

        public void Dispose()
        {
            Sql.Close();
            ((IDisposable)Sql).Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
