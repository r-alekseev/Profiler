using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Profiler.Demo.Stopwatch
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = ContainerBootstrapper.Build();

            var profile = serviceProvider.GetService<IProfiler>();

            var threads = new List<Thread>();
            for (int i = 0; i < 3; i++)
            {
                var thread = new Thread(() =>
                {
                    TimeSpan delay = TimeSpan.FromMilliseconds(i * 100);
                    using (var section = profile.Section("section.{i}.{delay}", i, delay))
                    {
                        Thread.Sleep(delay);

                        for (int j = 0; j < 3; j++)
                        {
                            TimeSpan innerDelay = TimeSpan.FromMilliseconds(j * 10);
                            using (section.Section("child.{j}.{innerDelay}", j, innerDelay))
                            {
                                Thread.Sleep(innerDelay);
                            }
                        }
                    }
                });
                threads.Add(thread);
                thread.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            profile.WriteReport();

            Console.ReadKey();
        }

        public class ConsoleTraceWriter : ITraceWriter
        {
            public void Write(TimeSpan elapsed, string[] chain, params object[] args)
            {
                Console.WriteLine($"{string.Join(" -> ", chain)}: {elapsed.TotalMilliseconds} ms");
            }
        }

        public class ConsoleReportWriter : IReportWriter
        {
            private readonly ConcurrentBag<ISectionMetrics> _bag = new ConcurrentBag<ISectionMetrics>();

            public void Add(ISectionMetrics metrics) => _bag.Add(metrics);

            public void Write()
            {
                Console.WriteLine("metrics:");

                foreach (var metrics in _bag)
                {
                    Console.WriteLine($"{string.Join(" -> ", metrics.Chain)}: {metrics.Elapsed.TotalMilliseconds} ms ({metrics.Count} times)");
                }
            }
        }
    }
}
