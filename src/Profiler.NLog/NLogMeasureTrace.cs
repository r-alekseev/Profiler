using NLog;
using System;

namespace Profiler
{
    public class NLogMeasureTrace : IMeasureTrace
    {
        private readonly ILogger _logger;
        private readonly LogLevel _logLevel;
        private readonly string _timeSpanFormat;

        public NLogMeasureTrace(ILogger logger, LogLevel logLevel, string timeSpanFormat)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logLevel = logLevel ?? throw new ArgumentNullException(nameof(logLevel));

            _timeSpanFormat = timeSpanFormat;
        }

        public NLogMeasureTrace(ILogger logger, string timeSpanFormat = "g")
            : this(logger, LogLevel.Trace, timeSpanFormat)
        {
        }

        public void Log(TimeSpan elapsed, string format, params object[] args)
        {
            string message = $"[{elapsed.ToString(_timeSpanFormat)}] {string.Format(format, args)}";
            _logger.Log(_logLevel, message);
        }
    }
}