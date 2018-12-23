using System;

namespace Profiler
{
    public interface IMeasureTrace
    {
        void Log(TimeSpan elapsed, string format, params object[] args);
    }
}
