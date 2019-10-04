
namespace Profiler
{
    public static class CustomFactorySettingsExtensions
    {
        public static void UseStopwatchTimeMeasure(this ICustomFactorySettings settings)
        {
            settings.CreateTimeMeasure = () => new StopwatchTimeMeasure();
        }

        public static void UseDummyTraceWriter(this ICustomFactorySettings settings)
        {
            settings.CreateTraceWriter = () => DummyTraceWriter.Instance;
        }

        public static void UseDummyReportWriter(this ICustomFactorySettings settings)
        {
            settings.CreateReportWriter = () => DummyReportWriter.Instance;
        }
    }
}
