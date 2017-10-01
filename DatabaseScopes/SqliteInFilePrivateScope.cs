using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Threading;
using NHibernate;
using NHibernate.Tool.hbm2ddl;

namespace NHibernateDatabaseScope.DatabaseScopes
{
    public class SqliteInFilePrivateScope : IDatabaseScope
    {
        private static ISessionFactory sessionFactory;
        private static NHibernate.Cfg.Configuration configuration;
        private static string masterDatabaseFileName = "master.db";

        public static Func<string, NHibernate.Cfg.Configuration> BuildConfiguration;

        private string privateDatabaseFileName;
        private IDbConnection privateConnection;

        private static string BuildConnectionString(string databaseName)
        {
            return string.Format("Data Source={0};Version=3;New=True;", databaseName);
        }

        private NHibernate.Cfg.Configuration BuildConfigurationInternal(string connectionString)
        {
            if (BuildConfiguration == null)
            {
                throw new InvalidOperationException("BuildConfiguration delegate was not initialized.");
            }

            return BuildConfiguration(connectionString);
        }

        public SqliteInFilePrivateScope()
        {
            EnsureMasterDatabaseExistence();

            // to avoid name collisions in parallel scenario
            this.privateDatabaseFileName = Guid.NewGuid().ToString() + ".db";

            File.Copy(masterDatabaseFileName, this.privateDatabaseFileName);

            string connectionString = BuildConnectionString(this.privateDatabaseFileName);
            this.privateConnection = new SQLiteConnection(connectionString); ;
            this.privateConnection.Open();
        }

        private static object configurationSync = new object();

        private void EnsureMasterDatabaseExistence()
        {
            // to support parallel execution of unit tests
            lock (configurationSync)
            {
                if (configuration == null)
                {
                    string connectionString = BuildConnectionString(masterDatabaseFileName);

                    configuration = BuildConfigurationInternal(connectionString);
                    sessionFactory = configuration.BuildSessionFactory();

                    SchemaExport schemaExport = new SchemaExport(configuration);
                    schemaExport.Execute(false, true, false);
                }
            }
        }

        public ISession OpenSession()
        {
            ISession session = sessionFactory.OpenSession(this.privateConnection);

            return session;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.privateConnection != null)
                {
                    this.privateConnection.Close();
                    this.privateConnection.Dispose();
                }

                if (!string.IsNullOrEmpty(this.privateDatabaseFileName))
                {
                    TryDeleteFile(this.privateDatabaseFileName);
                }
            }
        }

        private void TryDeleteFile(string fileName)
        {
            bool deleted;
            int attemptsCount = 0;
            int maxAttemptsCount = 5;

            if (File.Exists(fileName))
            {
                // The private file is sometimes locked on the file system even
                // when the connection is closed. It is unlocked by the system
                // after a short time but the exception is still thrown.
                // I don't understand the issue at all but this solves it at least on my computer (powered by Windows 7, 32 bit).
                do
                {
                    try
                    {
                        File.Delete(fileName);
                        deleted = true;
                    }
                    catch (IOException)
                    {
                        deleted = false;
                        Thread.Sleep(1);
                    }

                    attemptsCount++;
                    if (attemptsCount > maxAttemptsCount)
                    {
                        throw new IOException(string.Format("File {0} cannot be deleted.", fileName));
                    }

                } while (!deleted);
            }
        }
    }
}
