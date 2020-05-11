using System;

namespace Profiler
{
    public static class ProfilerConfigurationExtensions
    {
        public static IProfiler CreateProfiler(this IProfilerConfiguration settings)
        {
            return new Profiler(new CustomFactory(settings));
        }


        public static IProfilerConfiguration UseTimeMeasure(this IProfilerConfiguration settings, Func<ITimeMeasure> create)
        {
            settings.CreateTimeMeasure = create;
            return settings;
        }

        public static IProfilerConfiguration UseTraceWriter(this IProfilerConfiguration settings, Func<ITraceWriter> create)
        {
            settings.CreateTraceWriter = create;
            return settings;
        }

        public static IProfilerConfiguration UseReportWriter(this IProfilerConfiguration settings, Func<IReportWriter> create)
        {
            settings.CreateReportWriter = create;
            return settings;
        }


        public static IProfilerConfiguration UseStopwatchTimeMeasure(this IProfilerConfiguration settings) => settings
            .UseTimeMeasure(create: () => new StopwatchTimeMeasure());

        public static IProfilerConfiguration UseDummyTraceWriter(this IProfilerConfiguration settings) => settings
            .UseTraceWriter(create: () => DummyTraceWriter.Instance);

        public static IProfilerConfiguration UseDummyReportWriter(this IProfilerConfiguration settings) => settings
            .UseReportWriter(create: () => DummyReportWriter.Instance);

        public static IProfilerConfiguration UseDebugTraceWriter(this IProfilerConfiguration settings) => settings
            .UseTraceWriter(create: () => DebugTraceWriter.Instance);
    }
}
