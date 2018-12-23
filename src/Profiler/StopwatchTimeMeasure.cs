using System;
using System.Diagnostics;

namespace Profiler
{
    public class StopwatchTimeMeasure : ITimeMeasure
    {
        private readonly Stopwatch _stopwatch;

        public StopwatchTimeMeasure()
        {
            _stopwatch = new Stopwatch();
        }

        public TimeSpan? Elapsed => _stopwatch.Elapsed;

        public void Start()
        {
            _stopwatch.Start();
        }

        public void Stop()
        {
            _stopwatch.Stop();
        }
    }
}