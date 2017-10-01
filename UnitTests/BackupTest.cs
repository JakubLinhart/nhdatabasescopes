using System.Data.SQLite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernateDatabaseScope.DatabaseScopes;

namespace NHibernateDatabaseScope.UnitTests
{
    [TestClass]
    public class BackupTest
    {
        private long GetTableCount(SQLiteConnection connection)
        {
            using (var command = new SQLiteCommand("SELECT count(*) FROM sqlite_master WHERE type = \"table\"", connection))
            {
                long tableCount = (long)command.ExecuteScalar();
                return tableCount;
            }
        }

        private void CreateTable(SQLiteConnection connection)
        {
            using (var createTableCommand = new SQLiteCommand("create table Table1 (id INTEGER PRIMARY KEY, data TEXT);", connection))
            {
                createTableCommand.ExecuteNonQuery();
            }
        }

        private const string connectionString = "Data Source=:memory:;Version=3;New=True;";

        [TestMethod]
        public void DatabaseStructureBackupTest()
        {
            var sourceConnection = new SQLiteConnection(connectionString);
            sourceConnection.Open();
            Assert.AreEqual(0, GetTableCount(sourceConnection));

            var destinationConnection = new SQLiteConnection(connectionString);
            destinationConnection.Open();
            Assert.AreEqual(0, GetTableCount(destinationConnection));

            CreateTable(sourceConnection);
            Assert.AreEqual(1, GetTableCount(sourceConnection));
            Assert.AreEqual(0, GetTableCount(destinationConnection));

            SqliteBackup.Backup(sourceConnection, destinationConnection);
            Assert.AreEqual(1, GetTableCount(destinationConnection));

            sourceConnection.Dispose();
            destinationConnection.Dispose();
        }

        [TestMethod]
        public void DataBackupTest()
        {
            var sourceConnection = new SQLiteConnection(connectionString);
            sourceConnection.Open();
            CreateTable(sourceConnection);

            using (var insertCommand = new SQLiteCommand("insert into Table1 (id, data) values (1, 'some test value')", sourceConnection))
            {
                int affectedRows = insertCommand.ExecuteNonQuery();
                Assert.AreEqual(1, affectedRows);
            }

            var destinationConnection = new SQLiteConnection(connectionString);
            destinationConnection.Open();
            
            SqliteBackup.Backup(sourceConnection, destinationConnection);

            using (var selectCommand = new SQLiteCommand("select id, data from Table1", destinationConnection))
            {
                using (var reader = selectCommand.ExecuteReader())
                {
                    bool success = reader.Read();
                    Assert.IsTrue(success);

                    Assert.AreEqual(1, (long)reader["id"]);
                    Assert.AreEqual("some test value", (string)reader["data"]);
                }
            }
        }
    }
}
