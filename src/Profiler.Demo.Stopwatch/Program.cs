using System;
using System.Collections.Generic;
using System.Threading;

namespace Profiler.Demo.Stopwatch
{
    class Program
    {
        static void Main(string[] args)
        {
            var provider = new SectionProvider(new Factory());

            Console.WriteLine("trace:");

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

            Console.WriteLine();
            Console.WriteLine("metrics:");
            provider.WriteReport();

            Console.ReadKey();
        }

        public class ConsoleTraceWriter : ITraceWriter
        {
            public void Write(int threadId, TimeSpan elapsed, string[] chain, params object[] args)
            {
                Console.WriteLine($"[thread # {threadId}] {string.Join(" -> ", chain)}: {elapsed.TotalMilliseconds} ms");
            }
        }

        public class ConsoleReportWriter : IReportWriter
        {
            private readonly List<ISectionMetrics> _list = new List<ISectionMetrics>();

            public void Add(ISectionMetrics metrics) => _list.Add(metrics);

            public void Write()
            {
                foreach(var metrics in _list)
                {
                    Console.WriteLine($"[thread # {metrics.ThreadId}] {string.Join(" -> ", metrics.Chain)}: {metrics.Elapsed.TotalMilliseconds} ms ({metrics.Count} times)");
                }
            }
        }

        public class Factory : IFactory
        {
            public IReportWriter CreateReportWriter() => new ConsoleReportWriter();
            public ITimeMeasure CreateTimeMeasure() => new StopwatchTimeMeasure();
            public ITraceWriter CreateTraceWriter() => new ConsoleTraceWriter();
        }
    }
}
