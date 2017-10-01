using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using NHibernate;
using NHibernate.Tool.hbm2ddl;

namespace NHibernateDatabaseScope.DatabaseScopes
{
    public class SqliteInMemorySharedScope : IDatabaseScope
    {
        private static ISessionFactory sessionFactory;
        private static NHibernate.Cfg.Configuration configuration;
        private static IDbConnection connection;

        public static Func<string, NHibernate.Cfg.Configuration> BuildConfiguration;

        private const string connectionString = "Data Source=:memory:;Version=3;New=True;";

        private NHibernate.Cfg.Configuration BuildConfigurationInternal()
        {
            if (BuildConfiguration == null)
            {
                throw new InvalidOperationException("BuildConfiguration delegate was not initialized.");
            }

            return BuildConfiguration(connectionString);
        }

        public SqliteInMemorySharedScope()
        {
            if (configuration == null)
            {
                configuration = BuildConfigurationInternal();
            }

            if (sessionFactory == null)
            {
                sessionFactory = configuration.BuildSessionFactory();
            }

            if (connection == null)
            {
                connection = new SQLiteConnection(connectionString);
                connection.Open();
            }

            var schemaExport = new SchemaExport(configuration);
            schemaExport.Execute(false, true, false, connection, TextWriter.Null);
        }

        public ISession OpenSession()
        {
            ISession session = sessionFactory.OpenSession(connection);

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
                // nothing to do
            }
        }
    }
}
