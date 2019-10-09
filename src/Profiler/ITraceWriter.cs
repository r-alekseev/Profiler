using System;

namespace Profiler
{
    public interface ITraceWriter
    {
        void Write(TimeSpan elapsed, string[] chain, params object[] args);
    }
}
