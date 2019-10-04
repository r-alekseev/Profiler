using System;

namespace Profiler
{
    public class CustomFactorySettings : ICustomFactorySettings
    {
        public Func<ITimeMeasure> CreateTimeMeasure { get; set; }
        public Func<ITraceWriter> CreateTraceWriter { get; set; }
        public Func<IReportWriter> CreateReportWriter { get; set; }
    }
}
