using System.Diagnostics;
using NHibernateDatabaseScope.DatabaseScopes;

namespace NHibernateDatabaseScope.PerformanceMeasurement
{
    public static class DatabaseScopeMeasurement
    {
        public static MeasurementResult Measure(IDatabaseScope scope, int totalCycles)
        {
            var firstSession = scope.OpenSession();
            firstSession.Dispose();

            long totalTicks = 0;

            for (int i = 0; i < totalCycles; i++)
            {
                var watch = Stopwatch.StartNew();
                
                scope.OpenSession();
                scope.Dispose();

                watch.Stop();

                totalTicks += watch.Elapsed.Ticks;
            }

            var result = new MeasurementResult(totalTicks, totalCycles);

            return result;
        }
    }
}
