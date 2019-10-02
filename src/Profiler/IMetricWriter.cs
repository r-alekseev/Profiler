using System;

namespace Profiler
{
    public interface IMetricWriter
    {
        void Write(int threadId, TimeSpan elapsed, int count, string format);
    }
}
