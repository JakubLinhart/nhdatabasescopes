using System;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernateDatabaseScope.SampleDomainModel;
using NHibernateDatabaseScope.DatabaseScopes;

namespace NHibernateDatabaseScope.PerformanceMeasurement
{
    class Program
    {
        private const int totalCycles = 100;

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

        static void Main(string[] args)
        {
            SqliteInMemorySharedScope.BuildConfiguration = BuildSqliteConfiguration;
            SqliteInMemoryPrivateScope.BuildConfiguration = BuildSqliteConfiguration;
            SqliteInFilePrivateScope.BuildConfiguration = BuildSqliteConfiguration;
            MsSqlCeInFilePrivateScope.BuildConfiguration = BuildMsSqlCeConfiguration;

            Measure(new SqliteInMemorySharedScope());
            Measure(new SqliteInMemoryPrivateScope());
            Measure(new SqliteInFilePrivateScope());
            Measure(new MsSqlCeInFilePrivateScope());
        }

        private static void Measure(IDatabaseScope scope)
        {
            var result = DatabaseScopeMeasurement.Measure(scope, totalCycles);
            WriteResults(scope.GetType().Name, result);
        }

        private static void WriteResults(string scopeName, MeasurementResult result)
        {
            Console.WriteLine("{0}:", scopeName);
            Console.WriteLine("total time =\t\t\t {0} ms", result.TotalTime.TotalMilliseconds);
            Console.WriteLine("average time per cycle =\t {0} ms", result.AverageTimePerCycle.TotalMilliseconds);
            Console.WriteLine("average cycles per second =\t {0} cycles/seconds", result.AverageCyclesPerSecond);
            Console.WriteLine();
        }
    }
}
