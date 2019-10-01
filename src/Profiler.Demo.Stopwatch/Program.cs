using System;
using System.Threading;
using System.Threading.Tasks;

namespace Profiler.Demo.Stopwatch
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var provider = new SectionProvider(
                getTimeMeasure: () => new StopwatchTimeMeasure(),
                traceWriter: new ConsoleTraceWriter());

            for (int i = 0; i < 3; i++)
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
            }

            Console.ReadKey();
        }

        public class ConsoleTraceWriter : ITraceWriter
        {
            public void Write(TimeSpan elapsed, string format, params object[] args)
            {
                Console.WriteLine($"{elapsed}: {format} ({string.Join(",", args)})");
            }
        }
    }
}
