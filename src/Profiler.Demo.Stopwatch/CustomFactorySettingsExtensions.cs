using static Profiler.Demo.Stopwatch.Program;

namespace Profiler.Demo.Stopwatch
{
    public static class CustomFactorySettingsExtensions
    {
        public static void UseConsoleTraceWriter(this ICustomFactorySettings settings)
        {
            settings.CreateTraceWriter = () => new ConsoleTraceWriter();
        }

        public static void UseConsoleReportWriter(this ICustomFactorySettings settings)
        {
            settings.CreateReportWriter = () => new ConsoleReportWriter();
        }
    }
}
