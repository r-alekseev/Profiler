using System;
using System.Diagnostics;

namespace Profiler
{
    public class StopwatchTimeMeasure : ITimeMeasure
    {
        private readonly Stopwatch _stopwatch;

        private TimeSpan _from;

        public StopwatchTimeMeasure()
        {
            _stopwatch = new Stopwatch();
        }

        public TimeSpan Total => _stopwatch.Elapsed;

        public TimeSpan Pause()
        {
            _stopwatch.Stop();
            TimeSpan elapsed = _stopwatch.Elapsed - _from;
            return elapsed;
        }

        public void Start()
        {
            _from = _stopwatch.Elapsed;
            _stopwatch.Start();
        }
    }
}
