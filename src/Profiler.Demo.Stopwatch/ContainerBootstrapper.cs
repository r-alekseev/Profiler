using Microsoft.Extensions.DependencyInjection;
using Profiler.DependencyInjection;

namespace Profiler.Demo.Stopwatch
{
    public static class ContainerBootstrapper
    {
        public static ServiceProvider Build(IFactory factory)
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddProfiler(factory);

            return serviceCollection.BuildServiceProvider();
        }
    }
}
