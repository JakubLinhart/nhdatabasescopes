using System;
using System.Data;
using System.Data.SqlServerCe;
using System.IO;
using System.Threading;
using NHibernate;
using NHibernate.Tool.hbm2ddl;

namespace NHibernateDatabaseScope.DatabaseScopes
{
    public class MsSqlCeInFilePrivateScope : IDatabaseScope
    {
        private static ISessionFactory sessionFactory;
        private static NHibernate.Cfg.Configuration configuration;
        private static string masterDatabaseFileName = "master.sdf";

        public static Func<string, NHibernate.Cfg.Configuration> BuildConfiguration;

        private string privateDatabaseFileName;
        private IDbConnection privateConnection;

        private static string BuildConnectionString(string databaseName)
        {
            return string.Format("Data Source={0}", databaseName);
        }

        private NHibernate.Cfg.Configuration BuildConfigurationInternal(string databaseName)
        {
            if (BuildConfiguration == null)
            {
                throw new InvalidOperationException("BuildConfiguration delegate was not initialized.");
            }

            string connectionString = BuildConnectionString(databaseName);

            return BuildConfiguration(connectionString);
        }

        public MsSqlCeInFilePrivateScope()
        {
            EnsureMasterDatabaseExistence();

            this.privateDatabaseFileName = Path.GetRandomFileName() + ".sdf";
            this.privateConnection = CreatePrivateConnection(this.privateDatabaseFileName);
            this.privateConnection.Open();
        }

        private static object masterDatabaseSync = new object();

        private void EnsureMasterDatabaseExistence()
        {
            if (configuration == null)
            {
                lock (masterDatabaseSync)
                {
                    if (configuration == null)
                    {
                        string connectionString = BuildConnectionString(masterDatabaseFileName);

                        if (File.Exists(masterDatabaseFileName))
                        {
                            File.Delete(masterDatabaseFileName);
                        }

                        using (var engine = new SqlCeEngine(connectionString))
                        {
                            engine.CreateDatabase();
                        }

                        configuration = BuildConfigurationInternal(masterDatabaseFileName);
                        sessionFactory = configuration.BuildSessionFactory();

                        using (var masterConnection = new SqlCeConnection(connectionString))
                        {
                            masterConnection.Open();

                            SchemaExport schemaExport = new SchemaExport(configuration);
                            schemaExport.Execute(false, true, false, masterConnection, TextWriter.Null);
                        }
                    }
                }
            }
        }

        public ISession OpenSession()
        {
            ISession session = sessionFactory.OpenSession(this.privateConnection);

            return session;
        }

        private IDbConnection CreatePrivateConnection(string databaseFileName)
        {
            File.Copy(masterDatabaseFileName, databaseFileName);

            string connectionString = BuildConnectionString(databaseFileName);
            var connection = new SqlCeConnection(connectionString);

            return connection;
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

            if (File.Exists(this.privateDatabaseFileName))
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
