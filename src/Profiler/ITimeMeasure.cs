using System;

namespace Profiler
{
    public interface ITimeMeasure
    {
        void Start();
        void Stop();
        TimeSpan? Elapsed { get; }
    }
}