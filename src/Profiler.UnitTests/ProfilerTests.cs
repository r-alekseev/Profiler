using System;
using System.Collections.Generic;
using System.Linq;
using Shouldly;
using Xunit;

namespace Profiler.Tests
{
    public class ProfilerTests
    {
        class StubTraceWriter : ITraceWriter
        {
            public readonly List<(TimeSpan Elapsed, string Format, object[] Args)> List = new List<(TimeSpan, string, object[])>();

            public void Write(TimeSpan elapsed, string[] chain, params object[] args)
            {
                List.Add((elapsed, string.Join(" -> ", chain), args));
            }
        }

        class StubReportWriter : IReportWriter
        {
            private readonly List<ISectionMetrics> _metricsList = new List<ISectionMetrics>();
            public Dictionary<string, List<ISectionMetrics>> Dictionary = new Dictionary<string, List<ISectionMetrics>>();

            public void Add(ISectionMetrics metrics)
            {
                _metricsList.Add(metrics);
            }

            public void Write()
            {
                Dictionary.Clear();

                foreach (var metrics in _metricsList)
                {
                    string key = string.Join(" -> ", metrics.Chain);
                    if (!Dictionary.TryGetValue(key, out List<ISectionMetrics> list))
                    {
                        list = new List<ISectionMetrics>();
                        Dictionary[key] = list;
                    }
                    list.Add(metrics);
                }
            }
        }

        class StubTimeMeasure : ITimeMeasure
        {
            private readonly Func<TimeSpan> _getElapsed;

            public StubTimeMeasure(Func<TimeSpan> getElapsed)
            {
                _getElapsed = getElapsed;
            }

            public TimeSpan Total { get; private set; }

            public TimeSpan Pause()
            {
                var elapsed = _getElapsed();
                Total += elapsed;
                return elapsed;
            }

            public void Start()
            {
            }
        }

        class StubFactory : IFactory
        {
            private readonly Func<ITimeMeasure> _getTimeMeasure;
            private readonly ITraceWriter _traceWriter;
            private readonly IReportWriter _reportWriter;

            public StubFactory(Func<ITimeMeasure> getTimeMeasure, ITraceWriter traceWriter, IReportWriter reportWriter)
            {
                _getTimeMeasure = getTimeMeasure;
                _traceWriter = traceWriter;
                _reportWriter = reportWriter;
            }

            public IReportWriter CreateReportWriter() => _reportWriter;
            public ITimeMeasure CreateTimeMeasure() => _getTimeMeasure();
            public ITraceWriter CreateTraceWriter() => _traceWriter;
        }


        [Fact]
        public void Profiler_Create_NullFactory_ShouldThrowArgumentNullException()
        {
            Should.Throw<ArgumentNullException>(() =>
            {
                new Profiler(null);
            });
        }

        [Fact]
        public void Profiler_Create_NullTraceWriter_ShouldThrowArgumentException()
        {
            Should.Throw<ArgumentException>(() =>
            {
                new Profiler(new StubFactory(() => new StubTimeMeasure(() => TimeSpan.Zero), null, new StubReportWriter()));
            });
        }

        [Fact]
        public void Profiler_Create_NullReportWriter_ShouldThrowArgumentException()
        {
            Should.Throw<ArgumentException>(() =>
            {
                new Profiler(new StubFactory(() => new StubTimeMeasure(() => TimeSpan.Zero), new StubTraceWriter(), null));
            });
        }

        [Fact]
        public void Profiler_Create_DummyTraceWriter_ShouldNotThrowExceptions()
        {
            new Profiler(new StubFactory(() => new StubTimeMeasure(() => TimeSpan.Zero), DummyTraceWriter.Instance, new StubReportWriter()));
        }

        [Fact]
        public void Profiler_Create_DummyReportWriter_ShouldNotThrowExceptions()
        {
            new Profiler(new StubFactory(() => new StubTimeMeasure(() => TimeSpan.Zero), new StubTraceWriter(), DummyReportWriter.Instance));
        }

        [Fact]
        public void Profiler_CreateSection_NullFormat_ShouldThrowArgumentException()
        {
            var profiler = new Profiler(new StubFactory(
                () => new StubTimeMeasure(() => TimeSpan.Zero), 
                new StubTraceWriter(),
                new StubReportWriter()));

            Should.Throw<ArgumentException>(() =>
            {
                profiler.Section(null);
            });
        }

        [Fact]
        public void Profiler_CreateSection_EmptyFormat_ShouldThrowArgumentException()
        {
            var profiler = new Profiler(new StubFactory(
                () => new StubTimeMeasure(() => TimeSpan.Zero),
                new StubTraceWriter(),
                new StubReportWriter()));

            Should.Throw<ArgumentException>(() =>
            {
                profiler.Section(string.Empty);
            });
        }

        [Fact]
        public void Profiler_CreateSection_AlreadyInUse_ShouldNotThrowExceptions()
        {
            var profiler = new Profiler(new StubFactory(
                () => new StubTimeMeasure(() => TimeSpan.Zero),
                new StubTraceWriter(),
                new StubReportWriter()));

            // first usage of 'test'
            using (profiler.Section("test"))
            {
                //Should.Throw<ArgumentException>(() =>
                //{
                    // thats fine.. no more exceptions
                    // the main reason is async operations
                    //  it's often a thread enters section, starts await and returns to the thread pool 
                    //  and before await ends and section been exited the thread enters the same-named section in another task
                    using (profiler.Section("test"))
                    {
                    }
                //});
            }
        }

        [Fact]
        public void Profiler_CreateSection_ReuseAfterDispose_ShouldNotThrowExceptions()
        {
            var profiler = new Profiler(new StubFactory(
                () => new StubTimeMeasure(() => TimeSpan.Zero),
                new StubTraceWriter(),
                new StubReportWriter()));

            // first usage of 'test'
            using (profiler.Section("test"))
            {
            }   // freeing 'test'

            // second usage of 'test'
            using (profiler.Section("test"))
            {
            }
        }

        [Fact]
        public void Profiler_CreateSection_UseSimilarNameForInnerSection_ShouldNotThrowExceptions()
        {
            var profiler = new Profiler(new StubFactory(
                () => new StubTimeMeasure(() => TimeSpan.Zero),
                new StubTraceWriter(),
                new StubReportWriter()));

            // usage of 'test'
            using (var section = profiler.Section("test"))
            {
                // usage of 'test -> test'
                using (section.Section("test"))
                {
                }
            }
        }

        [Fact]
        public void Profiler_CreateSection_DisposeSectionTwice_ShouldWriteTraceOnce()
        {
            var traceWriter = new StubTraceWriter();
            var reportWriter = new StubReportWriter();

            var profiler = new Profiler(new StubFactory(() => new StubTimeMeasure(() => TimeSpan.Zero), traceWriter, reportWriter));

            var section = profiler.Section("section");

            section.Dispose();
            section.Dispose();

            traceWriter.List.Count.ShouldBe(1);
        }

        [Fact]
        public void Profiler_CreateAndDisposeSection_SingleTests()
        {
            var traceWriter = new StubTraceWriter();
            var reportWriter = new StubReportWriter();
            
            Func<TimeSpan> getElapsed = () => TimeSpan.FromMilliseconds(123);

            var profiler = new Profiler(new StubFactory(() => new StubTimeMeasure(getElapsed), traceWriter, reportWriter));

            traceWriter.List.Count.ShouldBe(0);

            using (profiler.Section("section"))
            {
                traceWriter.List.Count.ShouldBe(0);
            }

            traceWriter.List.Count.ShouldBe(1);
            traceWriter.List[0].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(123));
            traceWriter.List[0].Format.ShouldBe("section");

            profiler.WriteReport();

            reportWriter.Dictionary.Count.ShouldBe(1);
            reportWriter.Dictionary.ContainsKey("section").ShouldBe(true);
            reportWriter.Dictionary["section"].All(m => string.Join(" -> ", m.Chain) == "section").ShouldBe(true);
            reportWriter.Dictionary["section"].Sum(m => m.Elapsed.TotalMilliseconds).ShouldBe(123);
            reportWriter.Dictionary["section"].Sum(m => m.Count).ShouldBe(1);
        }

        [Fact]
        public void Profiler_CreateAndDisposeSection_SequentionalTests()
        {
            var traceWriter = new StubTraceWriter();
            var reportWriter = new StubReportWriter();

            int index = 0;
            int[] milliseconds = new[] { 123, 45, 6 };

            Func<TimeSpan> getElapsed = () => TimeSpan.FromMilliseconds(milliseconds[index++]);

            var profiler = new Profiler(new StubFactory(() => new StubTimeMeasure(getElapsed), traceWriter, reportWriter));

            traceWriter.List.Count.ShouldBe(0);

            using (profiler.Section("section.one"))
            {
                traceWriter.List.Count.ShouldBe(0);
            }

            traceWriter.List.Count.ShouldBe(1);
            traceWriter.List[0].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(123));
            traceWriter.List[0].Format.ShouldBe("section.one");
            traceWriter.List[0].Args.Length.ShouldBe(0);

            using (profiler.Section("section.two"))
            {
                traceWriter.List.Count.ShouldBe(1);
            }

            traceWriter.List.Count.ShouldBe(2);
            traceWriter.List[0].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(123));
            traceWriter.List[0].Format.ShouldBe("section.one");
            traceWriter.List[0].Args.Length.ShouldBe(0);
            traceWriter.List[1].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(45));
            traceWriter.List[1].Format.ShouldBe("section.two");
            traceWriter.List[1].Args.Length.ShouldBe(0);

            using (profiler.Section("section.three"))
            {
                traceWriter.List.Count.ShouldBe(2);
            }

            traceWriter.List.Count.ShouldBe(3);
            traceWriter.List[0].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(123));
            traceWriter.List[0].Format.ShouldBe("section.one");
            traceWriter.List[0].Args.Length.ShouldBe(0);
            traceWriter.List[1].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(45));
            traceWriter.List[1].Format.ShouldBe("section.two");
            traceWriter.List[1].Args.Length.ShouldBe(0);
            traceWriter.List[2].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(6));
            traceWriter.List[2].Format.ShouldBe("section.three");
            traceWriter.List[2].Args.Length.ShouldBe(0);

            profiler.WriteReport();

            reportWriter.Dictionary.Count.ShouldBe(3);
            reportWriter.Dictionary.ContainsKey("section.one").ShouldBe(true);
            reportWriter.Dictionary["section.one"].All(m => string.Join(" -> ", m.Chain) == "section.one").ShouldBe(true);
            reportWriter.Dictionary["section.one"].Sum(m => m.Elapsed.TotalMilliseconds).ShouldBe(123);
            reportWriter.Dictionary["section.one"].Sum(m => m.Count).ShouldBe(1);
            reportWriter.Dictionary.ContainsKey("section.two").ShouldBe(true);
            reportWriter.Dictionary["section.two"].All(m => string.Join(" -> ", m.Chain) == "section.two").ShouldBe(true);
            reportWriter.Dictionary["section.two"].Sum(m => m.Elapsed.TotalMilliseconds).ShouldBe(45);
            reportWriter.Dictionary["section.two"].Sum(m => m.Count).ShouldBe(1);
            reportWriter.Dictionary.ContainsKey("section.three").ShouldBe(true);
            reportWriter.Dictionary["section.three"].All(m => string.Join(" -> ", m.Chain) == "section.three").ShouldBe(true);
            reportWriter.Dictionary["section.three"].Sum(m => m.Elapsed.TotalMilliseconds).ShouldBe(6);
            reportWriter.Dictionary["section.three"].Sum(m => m.Count).ShouldBe(1);
        }

        [Fact]
        public void Profiler_CreateAndDisposeSection_RepeatingTests()
        {
            var traceWriter = new StubTraceWriter();
            var reportWriter = new StubReportWriter();

            int index = 0;
            int[] milliseconds = new[] { 123, 45, 6 };

            Func<TimeSpan> getElapsed = () => TimeSpan.FromMilliseconds(milliseconds[index++]);

            var profiler = new Profiler(new StubFactory(() => new StubTimeMeasure(getElapsed), traceWriter, reportWriter));

            traceWriter.List.Count.ShouldBe(0);

            for (int i = 0; i < 3; i++)
            {
                using (profiler.Section("section.{number}", i))
                {
                }
            }

            traceWriter.List.Count.ShouldBe(3);
            traceWriter.List[0].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(123));
            traceWriter.List[0].Format.ShouldBe("section.{number}");
            traceWriter.List[0].Args.Length.ShouldBe(1);
            traceWriter.List[0].Args[0].ShouldBe(0);

            traceWriter.List[1].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(45));
            traceWriter.List[1].Format.ShouldBe("section.{number}");
            traceWriter.List[1].Args.Length.ShouldBe(1);
            traceWriter.List[1].Args[0].ShouldBe(1);

            traceWriter.List[2].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(6));
            traceWriter.List[2].Format.ShouldBe("section.{number}");
            traceWriter.List[2].Args.Length.ShouldBe(1);
            traceWriter.List[2].Args[0].ShouldBe(2);

            profiler.WriteReport();

            reportWriter.Dictionary.Count.ShouldBe(1);
            reportWriter.Dictionary.ContainsKey("section.{number}").ShouldBe(true);
            reportWriter.Dictionary["section.{number}"].All(m => string.Join(" -> ", m.Chain) == "section.{number}").ShouldBe(true);
            reportWriter.Dictionary["section.{number}"].Sum(m => m.Elapsed.TotalMilliseconds).ShouldBe(174);
            reportWriter.Dictionary["section.{number}"].Sum(m => m.Count).ShouldBe(3);
        }

        [Fact]
        public void Profiler_CreateAndDisposeSection_WithChilds()
        {
            var traceWriter = new StubTraceWriter();
            var reportWriter = new StubReportWriter();

            int index = 0;
            int[] milliseconds = new[] { 123, 123, 300 };

            Func<TimeSpan> getElapsed = () => TimeSpan.FromMilliseconds(milliseconds[index++]);

            var profiler = new Profiler(new StubFactory(() => new StubTimeMeasure(getElapsed), traceWriter, reportWriter));

            traceWriter.List.Count.ShouldBe(0);

            using (var section = profiler.Section("outer"))
            {
                traceWriter.List.Count.ShouldBe(0);

                using (section.Section("inner"))
                {
                    traceWriter.List.Count.ShouldBe(0);
                }

                traceWriter.List.Count.ShouldBe(1);
                traceWriter.List[0].Format.ShouldBe("outer -> inner");
                traceWriter.List[0].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(123));

                using (section.Section("inner"))
                {
                    traceWriter.List.Count.ShouldBe(1);
                }

                traceWriter.List.Count.ShouldBe(2);
                traceWriter.List[0].Format.ShouldBe("outer -> inner");
                traceWriter.List[0].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(123));
                traceWriter.List[1].Format.ShouldBe("outer -> inner");
                traceWriter.List[1].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(123));
            }

            traceWriter.List.Count.ShouldBe(3);
            traceWriter.List[0].Format.ShouldBe("outer -> inner");
            traceWriter.List[0].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(123));
            traceWriter.List[1].Format.ShouldBe("outer -> inner");
            traceWriter.List[1].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(123));
            traceWriter.List[2].Format.ShouldBe("outer");
            traceWriter.List[2].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(300));

            profiler.WriteReport();

            reportWriter.Dictionary.Count.ShouldBe(2);
            reportWriter.Dictionary.ContainsKey("outer").ShouldBe(true);
            reportWriter.Dictionary["outer"].All(m => string.Join(" -> ", m.Chain) == "outer").ShouldBe(true);
            reportWriter.Dictionary["outer"].Sum(m => m.Elapsed.TotalMilliseconds).ShouldBe(300);
            reportWriter.Dictionary["outer"].Sum(m => m.Count).ShouldBe(1);
            reportWriter.Dictionary.ContainsKey("outer -> inner").ShouldBe(true);
            reportWriter.Dictionary["outer -> inner"].All(m => string.Join(" -> ", m.Chain) == "outer -> inner").ShouldBe(true);
            reportWriter.Dictionary["outer -> inner"].Sum(m => m.Elapsed.TotalMilliseconds).ShouldBe(246);
            reportWriter.Dictionary["outer -> inner"].Sum(m => m.Count).ShouldBe(2);
        }
    }
}