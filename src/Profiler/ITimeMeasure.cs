using System;

namespace Profiler
{
    public interface ITimeMeasure
    {
        void Start();
        TimeSpan Pause();
        TimeSpan Total { get; }
    }
}
