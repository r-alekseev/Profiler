using System;
using System.Collections.Generic;
using System.Threading;

namespace Profiler.Demo.Stopwatch
{
    class Program
    {
        static void Main(string[] args)
        {
            var provider = new SectionProvider(
                getTimeMeasure: () => new StopwatchTimeMeasure(),
                traceWriter: new ConsoleTraceWriter(),
                metricWriter: new ConsoleMetricsWriter());

            var threads = new List<Thread>();
            for (int i = 0; i < 3; i++)
            {
                var thread = new Thread(() =>
                {
                    TimeSpan delay = TimeSpan.FromMilliseconds(i * 100);
                    using (var section = provider.Section("section.{i}.{delay}", i, delay))
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

            provider.Flush();

            Console.ReadKey();
        }

        public class ConsoleTraceWriter : ITraceWriter
        {
            public void Write(int threadId, TimeSpan elapsed, string format, params object[] args)
            {
                Console.WriteLine($"[{threadId}] {elapsed}: {format} ({string.Join(",", args)})");
            }
        }

        public class ConsoleMetricsWriter : IMetricWriter
        {
            public void Write(int threadId, TimeSpan elapsed, string format)
            {
                Console.WriteLine($"[{threadId}] {format}: {elapsed}");
            }
        }
    }
}
