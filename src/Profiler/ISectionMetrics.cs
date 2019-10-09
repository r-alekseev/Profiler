using System;

namespace Profiler
{
    public interface ISectionMetrics
    {
        TimeSpan Elapsed { get; }
        int Count { get; }
        string[] Chain { get; }
    }
}
