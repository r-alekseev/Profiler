using System;

namespace Profiler
{
    public interface IProfilerConfiguration
    {
        Func<ITimeMeasure> CreateTimeMeasure { get; set; }
        Func<ITraceWriter> CreateTraceWriter { get; set; }
        Func<IReportWriter> CreateReportWriter { get; set; }
    }
}
