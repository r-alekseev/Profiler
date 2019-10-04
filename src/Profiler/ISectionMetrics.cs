using System;

namespace Profiler
{
    public interface ISectionMetrics
    {
        int ThreadId { get; }
        TimeSpan Elapsed { get; }
        int Count { get; }
        string[] Chain { get; }
    }
}
