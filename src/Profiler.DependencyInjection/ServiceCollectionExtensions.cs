using Microsoft.Extensions.DependencyInjection;

namespace Profiler.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddProfiler(
            this ServiceCollection serviceCollection,
            IFactory factory)
        {
            serviceCollection.AddSingleton<IFactory>(factory);
            serviceCollection.AddSingleton<ISectionProvider, SectionProvider>();
        }
    }
}
