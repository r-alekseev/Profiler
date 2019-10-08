﻿using Profiler;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddProfiler(
            this ServiceCollection serviceCollection,
            IFactory factory)
        {
            if (serviceCollection == null) throw new ArgumentNullException(nameof(serviceCollection));

            var profiler = new Profiler.Profiler(factory);
            serviceCollection.AddSingleton<IProfiler>(profiler);
        }

        public static void AddProfiler(
            this ServiceCollection serviceCollection,
            Action<ICustomFactorySettings> configure = null)
        {
            var settings = new CustomFactorySettings();
            configure?.Invoke(settings);
            var factory = new CustomFactory(settings);

            AddProfiler(serviceCollection, factory);
        }
    }
}
