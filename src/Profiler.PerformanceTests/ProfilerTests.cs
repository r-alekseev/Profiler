using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Profiler.PerformanceTests
{
    public class ProfilerTests
    {
        private readonly ITestOutputHelper _output;

        public ProfilerTests(ITestOutputHelper output)
        {
            _output = output;
        }

        class StubReportWriter : IReportWriter
        {
            private readonly ConcurrentQueue<ISectionMetrics> _queue = new ConcurrentQueue<ISectionMetrics>();
            public ConcurrentQueue<ISectionMetrics> Queue => _queue;

            public void Add(ISectionMetrics metrics)
            {
                _queue.Enqueue(metrics);
            }

            public void Write() { }
        }

        [Theory]
        //          count           keys        depth       parralel
        [InlineData(1_000_000,      1,          1,          1)]
        [InlineData(1_000_000,      100,        1,          1)]
        [InlineData(1_000_000,      10_000,     1,          1)]
        [InlineData(1_000_000,      1,          10,         1)]
        [InlineData(1_000_000,      100,        10,         1)]
        [InlineData(1_000_000,      10_000,     10,         1)]
        [InlineData(1_000_000,      1,          100,        1)]
        [InlineData(1_000_000,      100,        100,        1)]
        [InlineData(1_000_000,      10_000,     100,        1)]
        //[InlineData(1_000_000,      1,          1000,       1)]
        //[InlineData(1_000_000,      100,        1000,       1)]
        //[InlineData(1_000_000,      10_000,     1000,       1)]

        [InlineData(1_000_000,      1,          1,          10)]
        [InlineData(1_000_000,      100,        1,          10)]
        [InlineData(1_000_000,      10_000,     1,          10)]
        [InlineData(1_000_000,      1,          10,         10)]
        [InlineData(1_000_000,      100,        10,         10)]
        [InlineData(1_000_000,      10_000,     10,         10)]
        [InlineData(1_000_000,      1,          100,        10)]
        [InlineData(1_000_000,      100,        100,        10)]
        [InlineData(1_000_000,      10_000,     100,        10)]
        //[InlineData(1_000_000,      1,          1000,       10)]
        //[InlineData(1_000_000,      100,        1000,       10)]
        //[InlineData(1_000_000,      10_000,     1000,       10)]

        [InlineData(1_000_000,      1,          1,          100)]
        [InlineData(1_000_000,      100,        1,          100)]
        [InlineData(1_000_000,      10_000,     1,          100)]
        [InlineData(1_000_000,      1,          10,         100)]
        [InlineData(1_000_000,      100,        10,         100)]
        [InlineData(1_000_000,      10_000,     10,         100)]
        [InlineData(1_000_000,      1,          100,        100)]
        [InlineData(1_000_000,      100,        100,        100)]
        [InlineData(1_000_000,      10_000,     100,        100)]
        //[InlineData(1_000_000,      1,          1000,       100)]
        //[InlineData(1_000_000,      100,        1000,       100)]
        //[InlineData(1_000_000,      10_000,     1000,       100)]
        public void Profiler_CreateSections_Enter_Exit(int count, int keysCount, int depth, int maxDegreeOfParallelism)
        {
            var reportWriter = new StubReportWriter();

            var conf = new ProfilerConfiguration();
            conf.CreateReportWriter = () => reportWriter;
            var profiler = conf.CreateProfiler();

            string[] keys = new string[count];
            for (int i = 0; i < keysCount; i++) keys[i] = i.ToString();

            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism };

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            Parallel.For(0, count / depth, parallelOptions, i =>
            //for (int i = 0; i < count / depth; i++)
            {
                var section = profiler.Section(keys[i % keysCount]);
                ISection currentSection = section;
                Stack<ISection> sections = new Stack<ISection>(depth - 1);
                sections.Push(currentSection);
                while (sections.Count < depth)
                {
                    currentSection = currentSection.Section(keys[(i + sections.Count) % keysCount]);
                    sections.Push(currentSection);
                }
                while (sections.Count > 0)
                {
                    sections.Pop().Dispose();
                }
            });
            stopwatch.Stop();

            string log = $"count: {count}, keysCount: {keysCount}, depth: {depth}, maxDegreeOfParallelism: {maxDegreeOfParallelism}, elapsed: {stopwatch.ElapsedMilliseconds} ms, allocated {reportWriter.Queue.Count} sections";
            _output.WriteLine(log);
            Console.WriteLine(log);

            reportWriter.Queue.Sum(m => m.Count).ShouldBe(count);
        }

        [Theory]
        //          count           keys        depth   
        [InlineData(1_000_000,      1,          1       )]
        [InlineData(1_000_000,      100,        1       )]
        [InlineData(1_000_000,      10_000,     1       )]
        [InlineData(1_000_000,      1,          10      )]
        [InlineData(1_000_000,      100,        10      )]
        [InlineData(1_000_000,      10_000,     10      )]
        [InlineData(1_000_000,      1,          100     )]
        [InlineData(1_000_000,      100,        100     )]
        [InlineData(1_000_000,      10_000,     100     )]
        //[InlineData(1_000_000,      1,          1000    )]
        //[InlineData(1_000_000,      100,        1000    )]
        //[InlineData(1_000_000,      10_000,     1000    )]
        public async Task Profiler_CreateSections_Enter_AsyncYield_Exit(int count, int keysCount, int depth)
        {
            var reportWriter = new StubReportWriter();

            var conf = new ProfilerConfiguration();
            conf.CreateReportWriter = () => reportWriter;
            var profiler = conf.CreateProfiler();

            string[] keys = new string[count];
            for (int i = 0; i < keysCount; i++) keys[i] = i.ToString();

            int batchSize = 100;
            var tasks = new List<Task>(batchSize);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 0; i < (count / depth) / batchSize; i++)
            {
                tasks.Add(await Task.Factory.StartNew(async () =>
                {
                    for (int j = 0; j < batchSize; j++)
                    {
                        var section = profiler.Section(keys[i % keysCount]);
                        ISection currentSection = section;
                        Stack<ISection> sections = new Stack<ISection>(depth - 1);
                        sections.Push(currentSection);
                        while (sections.Count < depth)
                        {
                            currentSection = currentSection.Section(keys[(i + sections.Count) % keysCount]);
                            sections.Push(currentSection);
                            await Task.Yield();
                        }

                        while (sections.Count > 0)
                        {
                            await Task.Yield();
                            sections.Pop().Dispose();
                        }
                    }
                }));
            }
            await Task.WhenAll(tasks);

            stopwatch.Stop();

            string log = $"count: {count}, keysCount: {keysCount}, depth: {depth}, elapsed: {stopwatch.ElapsedMilliseconds} ms, allocated {reportWriter.Queue.Count} sections";
            _output.WriteLine(log);
            Console.WriteLine(log);

            reportWriter.Queue.Sum(m => m.Count).ShouldBe(count);
        }
    }
}
