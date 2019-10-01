using System;

namespace Profiler
{
    public interface ITraceWriter
    {
        void Write(TimeSpan elapsed, string format, params object[] args);
    }
}
