namespace Profiler
{
    public static class ProfilerConfigurationExtensions
    {
        public static IProfiler CreateProfiler(this IProfilerConfiguration settings)
        {
            return new Profiler(new CustomFactory(settings));
        }

        public static IProfilerConfiguration UseStopwatchTimeMeasure(this IProfilerConfiguration settings)
        {
            settings.CreateTimeMeasure = () => new StopwatchTimeMeasure();
            return settings;
        }

        public static IProfilerConfiguration UseDummyTraceWriter(this IProfilerConfiguration settings)
        {
            settings.CreateTraceWriter = () => DummyTraceWriter.Instance;
            return settings;
        }

        public static IProfilerConfiguration UseDummyReportWriter(this IProfilerConfiguration settings)
        {
            settings.CreateReportWriter = () => DummyReportWriter.Instance;
            return settings;
        }

        public static IProfilerConfiguration UseDebugTraceWriter(this IProfilerConfiguration settings)
        {
            settings.CreateTraceWriter = () => new DebugTraceWriter();
            return settings;
        }
    }
}
