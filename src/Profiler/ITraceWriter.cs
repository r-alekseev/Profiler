using System;

namespace Profiler
{
    public interface ITraceWriter
    {
        void Write(int threadId, TimeSpan elapsed, string[] chain, params object[] args);
    }
}
