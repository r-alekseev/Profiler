using Microsoft.Extensions.DependencyInjection;
using System;

namespace Profiler.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddProfiler(
            this ServiceCollection serviceCollection,
            IFactory factory)
        {
            if (serviceCollection == null) throw new ArgumentNullException(nameof(serviceCollection));

            serviceCollection.AddSingleton<IFactory>(factory);
            serviceCollection.AddSingleton<ISectionProvider, SectionProvider>();
        }

        public static void AddProfiler(
            this ServiceCollection serviceCollection,
            Action<ICustomFactorySettings> configure)
        {
            var settings = new CustomFactorySettings();
            configure(settings);
            var factory = new CustomFactory(settings);

            AddProfiler(serviceCollection, factory);
        }
    }
}
