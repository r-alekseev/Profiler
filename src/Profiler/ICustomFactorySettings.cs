using System;

namespace Profiler
{
    public interface ICustomFactorySettings
    {
        Func<ITimeMeasure> CreateTimeMeasure { get; set; }
        Func<ITraceWriter> CreateTraceWriter { get; set; }
        Func<IReportWriter> CreateReportWriter { get; set; }
    }
}
