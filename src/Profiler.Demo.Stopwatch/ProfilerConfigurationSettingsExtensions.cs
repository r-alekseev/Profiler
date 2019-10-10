using static Profiler.Demo.Stopwatch.Program;

namespace Profiler.Demo.Stopwatch
{
    public static class ProfilerConfigurationSettingsExtensions
    {
        public static void UseConsoleTraceWriter(this IProfilerConfiguration settings)
        {
            settings.CreateTraceWriter = () => new ConsoleTraceWriter();
        }

        public static void UseConsoleReportWriter(this IProfilerConfiguration settings)
        {
            settings.CreateReportWriter = () => new ConsoleReportWriter();
        }
    }
}
