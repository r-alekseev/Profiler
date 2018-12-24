using NLog;
using System;
using System.Threading;

namespace Profiler.NLog.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            LogManager.LoadConfiguration("nlog.config");

            var logger = LogManager.GetCurrentClassLogger();

            var profiler = new SectionProvider(
                getTimeMeasure: () => new StopwatchTimeMeasure(),
                measureTrace: new NLogMeasureTrace(logger));

            DoWork(profiler);

            LogManager.Shutdown();
            Console.WriteLine("Press any key to exit ..");
            Console.ReadKey();
        }

        private static void DoWork(ISectionProvider profiler)
        {
            using (var section = profiler.Section("dowork"))
            {
                Thread.Sleep(123);

                DoPartOfWork(section);

                Thread.Sleep(123);
            }
        }

        private static void DoPartOfWork(ISectionProvider profiler)
        {
            using (profiler.Section("dopartofwork"))
            {
                Thread.Sleep(321);
            }
        }
    }
}
