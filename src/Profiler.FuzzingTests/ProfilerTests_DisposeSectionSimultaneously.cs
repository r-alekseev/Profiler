using System;
using System.Threading;
using Xunit;
using Shouldly;
using System.Threading.Tasks;

namespace Profiler.FuzzingTests
{
    public partial class ProfilerTests
    {
        class StubTraceWriter : ITraceWriter
        {
            private int _count;

            public int Count => _count;

            public void Write(int threadId, TimeSpan elapsed, string[] chain, params object[] args)
            {
                Interlocked.Increment(ref _count);
            }
        }

        [Fact]
        public void Profiler_CreateSection_DisposeSectionSimultaneously_ShouldWriteTraceOnce()
        {
            var traceWriter = new StubTraceWriter();

            var settings = new CustomFactorySettings();
            settings.CreateTraceWriter = () => traceWriter;

            var factory = new CustomFactory(settings);

            var profiler = new Profiler(factory);

            var section = profiler.Section("section");

            int threadCount = 1_000_000;

            var tasks = new Task[threadCount];
            //var threadIds = new HashSet<int>();

            for (int i = 0; i < threadCount; i++)
            {
                tasks[i] = Task.Factory.StartNew(() =>
                {
                    // uncomment to ensure there are several threads

                    //lock (threadIds)
                    //{
                    //    threadIds.Add(Thread.CurrentThread.ManagedThreadId);
                    //}
                    section.Dispose();
                });
            }

            Task.WaitAll(tasks);

            traceWriter.Count.ShouldBe(1);
        }

    }
}
