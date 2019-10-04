using Microsoft.Extensions.DependencyInjection;
using Profiler.DependencyInjection;

namespace Profiler.Demo.Stopwatch
{
    public static class ContainerBootstrapper
    {
        public static ServiceProvider Build()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddProfiler(settings =>
            {
                settings.UseStopwatchTimeMeasure();
                settings.UseConsoleTraceWriter();
                settings.UseConsoleReportWriter();
            });

            return serviceCollection.BuildServiceProvider();
        }
    }
}