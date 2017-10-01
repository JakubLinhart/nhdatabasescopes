using System;
using System.Data.SQLite;
using System.IO;
using NHibernate;
using NHibernate.Tool.hbm2ddl;

namespace NHibernateDatabaseScope.DatabaseScopes
{
    public class SqliteInMemoryPrivateScope : IDatabaseScope
    {
        private static ISessionFactory sessionFactory;
        private static NHibernate.Cfg.Configuration configuration;
        private static SQLiteConnection masterConnection;

        private static object configurationSync = new object();

        public static Func<string, NHibernate.Cfg.Configuration> BuildConfiguration;

        private const string connectionString = "Data Source=:memory:;Version=3;New=True;";

        private SQLiteConnection privateConnection;

        private NHibernate.Cfg.Configuration BuildConfigurationInternal()
        {
            if (BuildConfiguration == null)
            {
                throw new InvalidOperationException("BuildConfiguration delegate was not initialized.");
            }

            return BuildConfiguration(connectionString);
        }

        public SqliteInMemoryPrivateScope()
        {
            EnsureMasterDatabaseExistence();

            this.privateConnection = new SQLiteConnection(connectionString);
            this.privateConnection.Open();

            SqliteBackup.Backup(masterConnection, this.privateConnection);
        }

        private void EnsureMasterDatabaseExistence()
        {
            // to support asynchronous scenario
            lock (configurationSync)
            {
                if (configuration == null)
                {
                    configuration = BuildConfigurationInternal();
                    sessionFactory = configuration.BuildSessionFactory();

                    masterConnection = new SQLiteConnection(connectionString);
                    masterConnection.Open();

                    SchemaExport schemaExport = new SchemaExport(configuration);
                    schemaExport.Execute(false, true, false, masterConnection, TextWriter.Null);
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
                    this.privateConnection.Dispose();
                    this.privateConnection = null;
                }
            }
        }
    }
}
