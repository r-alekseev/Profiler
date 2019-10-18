using static Profiler.Demo.Stopwatch.Program;

namespace Profiler.Demo.Stopwatch
{
    public static class ProfilerConfigurationSettingsExtensions
    {
        public static IProfilerConfiguration UseConsoleTraceWriter(this IProfilerConfiguration settings)
        {
            settings.CreateTraceWriter = () => new ConsoleTraceWriter();
            return settings;
        }

        public static IProfilerConfiguration UseConsoleReportWriter(this IProfilerConfiguration settings)
        {
            settings.CreateReportWriter = () => new ConsoleReportWriter();
            return settings;
        }
    }
}
