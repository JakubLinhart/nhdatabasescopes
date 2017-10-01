using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernateDatabaseScope.SampleDomainModel;
using NHibernateDatabaseScope.DatabaseScopes;

namespace NHibernateDatabaseScope.UnitTests
{
    [TestClass]
    public class TestEnvironmentInitialization
    {
        private static NHibernate.Cfg.Configuration BuildSqliteConfiguration(string connectionString)
        {
            var config = Fluently.Configure()
             .Database(SQLiteConfiguration.Standard.ConnectionString(connectionString))
             .Mappings(m =>
             {
                 m.FluentMappings.AddFromAssembly(typeof(Order).Assembly);
             })
             .BuildConfiguration();

            return config;
        }

        private static NHibernate.Cfg.Configuration BuildMsSqlCeConfiguration(string connectionString)
        {
            var config = Fluently.Configure()
             .Database(MsSqlCeConfiguration.Standard.ConnectionString(connectionString))
             .Mappings(m =>
             {
                 m.FluentMappings.AddFromAssembly(typeof(Order).Assembly);
             })
             .BuildConfiguration();

            return config;
        }

        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            SqliteInMemorySharedScope.BuildConfiguration = BuildSqliteConfiguration;
            SqliteInMemoryPrivateScope.BuildConfiguration = BuildSqliteConfiguration;
            SqliteInFilePrivateScope.BuildConfiguration = BuildSqliteConfiguration;

            MsSqlCeInFilePrivateScope.BuildConfiguration = BuildMsSqlCeConfiguration;
        }
    }
}
