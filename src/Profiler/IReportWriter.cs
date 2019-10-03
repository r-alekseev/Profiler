using System;

namespace Profiler
{
    public interface IReportWriter
    {
        void Write(int threadId, TimeSpan elapsed, int count, string format);
    }
}
