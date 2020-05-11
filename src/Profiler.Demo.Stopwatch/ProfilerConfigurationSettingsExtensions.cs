using static Profiler.Demo.Stopwatch.Program;

namespace Profiler.Demo.Stopwatch
{
    public static class ProfilerConfigurationSettingsExtensions
    {
        public static IProfilerConfiguration UseConsoleTraceWriter(this IProfilerConfiguration settings) => settings
            .UseTraceWriter(create: () => new ConsoleTraceWriter());

        public static IProfilerConfiguration UseConsoleReportWriter(this IProfilerConfiguration settings) => settings
            .UseReportWriter(create: () => new ConsoleReportWriter());
    }
}
