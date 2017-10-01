using System;

namespace NHibernateDatabaseScope.PerformanceMeasurement
{
    public class MeasurementResult
    {
        private long totalTicks;
        private long totalCycles;

        public MeasurementResult(long totalTicks, long totalCycles)
        {
            this.totalCycles = totalCycles;
            this.totalTicks = totalTicks;
        }

        public TimeSpan TotalTime
        {
            get
            {
                return new TimeSpan(this.totalTicks);
            }
        }

        public TimeSpan AverageTimePerCycle
        {
            get
            {
                return new TimeSpan(this.totalTicks / this.totalCycles);
            }
        }

        public double AverageCyclesPerSecond
        {
            get
            {
                return this.totalCycles / TotalTime.TotalSeconds;
            }
        }
    }
}
