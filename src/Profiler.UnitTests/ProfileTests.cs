using System;
using System.Collections.Generic;
using System.Linq;
using Shouldly;
using Xunit;

namespace Profiler.Tests
{
    public class ProfileTests
    {
        class StubTraceWriter : ITraceWriter
        {
            public readonly List<(TimeSpan Elapsed, string Format, object[] Args)> List = new List<(TimeSpan, string, object[])>();

            public void Write(int threadId, TimeSpan elapsed, string[] chain, params object[] args)
            {
                List.Add((elapsed, string.Join(" -> ", chain), args));
            }
        }

        class StubReportWriter : IReportWriter
        {
            private readonly List<ISectionMetrics> _inner = new List<ISectionMetrics>();
            public List<(TimeSpan Elapsed, int Count, string Format)> List = new List<(TimeSpan, int, string)>();

            public void Add(ISectionMetrics metrics)
            {
                _inner.Add(metrics);
            }

            public void Write()
            {
                List = _inner.Select(m => (m.Elapsed, m.Count, string.Join(" -> ", m.Chain))).ToList();
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
        public void Profile_Create_NullFactory_ShouldThrowArgumentNullException()
        {
            Should.Throw<ArgumentNullException>(() =>
            {
                new Profile(null);
            });
        }

        [Fact]
        public void Profile_Create_NullTraceWriter_ShouldThrowArgumentException()
        {
            Should.Throw<ArgumentException>(() =>
            {
                new Profile(new StubFactory(() => new StubTimeMeasure(() => TimeSpan.Zero), null, new StubReportWriter()));
            });
        }

        [Fact]
        public void Profile_Create_NullReportWriter_ShouldThrowArgumentException()
        {
            Should.Throw<ArgumentException>(() =>
            {
                new Profile(new StubFactory(() => new StubTimeMeasure(() => TimeSpan.Zero), new StubTraceWriter(), null));
            });
        }

        [Fact]
        public void Profile_Create_DummyTraceWriter_ShouldNotThrowExceptions()
        {
            new Profile(new StubFactory(() => new StubTimeMeasure(() => TimeSpan.Zero), DummyTraceWriter.Instance, new StubReportWriter()));
        }

        [Fact]
        public void Profile_Create_DummyReportWriter_ShouldNotThrowExceptions()
        {
            new Profile(new StubFactory(() => new StubTimeMeasure(() => TimeSpan.Zero), new StubTraceWriter(), DummyReportWriter.Instance));
        }

        [Fact]
        public void Profile_CreateSection_NullFormat_ShouldThrowArgumentException()
        {
            var profile = new Profile(new StubFactory(
                () => new StubTimeMeasure(() => TimeSpan.Zero), 
                new StubTraceWriter(),
                new StubReportWriter()));

            Should.Throw<ArgumentException>(() =>
            {
                profile.Section(null);
            });
        }

        [Fact]
        public void Profile_CreateSection_EmptyFormat_ShouldThrowArgumentException()
        {
            var profile = new Profile(new StubFactory(
                () => new StubTimeMeasure(() => TimeSpan.Zero),
                new StubTraceWriter(),
                new StubReportWriter()));

            Should.Throw<ArgumentException>(() =>
            {
                profile.Section(string.Empty);
            });
        }

        [Fact]
        public void Profile_CreateSection_AlreadyInUse_ShouldThrowArgumentException()
        {
            var profile = new Profile(new StubFactory(
                () => new StubTimeMeasure(() => TimeSpan.Zero),
                new StubTraceWriter(),
                new StubReportWriter()));

            // first usage of 'test'
            using (profile.Section("test"))
            {
                Should.Throw<ArgumentException>(() =>
                {
                    // already in use
                    using (profile.Section("test"))
                    {
                    }
                });
            }
        }

        [Fact]
        public void Profile_CreateSection_ReuseAfterDispose_ShouldNotThrowExceptions()
        {
            var profile = new Profile(new StubFactory(
                () => new StubTimeMeasure(() => TimeSpan.Zero),
                new StubTraceWriter(),
                new StubReportWriter()));

            // first usage of 'test'
            using (profile.Section("test"))
            {
            }   // freeing 'test'

            // second usage of 'test'
            using (profile.Section("test"))
            {
            }
        }

        [Fact]
        public void Profile_CreateSection_UseSimilarNameForInnerSection_ShouldNotThrowExceptions()
        {
            var profile = new Profile(new StubFactory(
                () => new StubTimeMeasure(() => TimeSpan.Zero),
                new StubTraceWriter(),
                new StubReportWriter()));

            // usage of 'test'
            using (var section = profile.Section("test"))
            {
                // usage of 'test -> test'
                using (section.Section("test"))
                {
                }
            }
        }

        [Fact]
        public void Profile_CreateAndDisposeSection_SingleTests()
        {
            var traceWriter = new StubTraceWriter();
            var reportWriter = new StubReportWriter();
            
            Func<TimeSpan> getElapsed = () => TimeSpan.FromMilliseconds(123);

            var profile = new Profile(new StubFactory(() => new StubTimeMeasure(getElapsed), traceWriter, reportWriter));

            traceWriter.List.Count.ShouldBe(0);

            using (profile.Section("section"))
            {
                traceWriter.List.Count.ShouldBe(0);
            }

            traceWriter.List.Count.ShouldBe(1);
            traceWriter.List[0].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(123));
            traceWriter.List[0].Format.ShouldBe("section");

            profile.WriteReport();

            reportWriter.List.Count.ShouldBe(1);
            reportWriter.List[0].Format.ShouldBe("section");
            reportWriter.List[0].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(123));
            reportWriter.List[0].Count.ShouldBe(1);
        }

        [Fact]
        public void Profile_CreateAndDisposeSection_SequentionalTests()
        {
            var traceWriter = new StubTraceWriter();
            var reportWriter = new StubReportWriter();

            int index = 0;
            int[] milliseconds = new[] { 123, 45, 6 };

            Func<TimeSpan> getElapsed = () => TimeSpan.FromMilliseconds(milliseconds[index++]);

            var profile = new Profile(new StubFactory(() => new StubTimeMeasure(getElapsed), traceWriter, reportWriter));

            traceWriter.List.Count.ShouldBe(0);

            using (profile.Section("section.one"))
            {
                traceWriter.List.Count.ShouldBe(0);
            }

            traceWriter.List.Count.ShouldBe(1);
            traceWriter.List[0].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(123));
            traceWriter.List[0].Format.ShouldBe("section.one");
            traceWriter.List[0].Args.Length.ShouldBe(0);

            using (profile.Section("section.two"))
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

            using (profile.Section("section.three"))
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

            profile.WriteReport();

            reportWriter.List.Count.ShouldBe(3);
            reportWriter.List[0].Format.ShouldBe("section.one");
            reportWriter.List[0].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(123));
            reportWriter.List[0].Count.ShouldBe(1);
            reportWriter.List[1].Format.ShouldBe("section.two");
            reportWriter.List[1].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(45));
            reportWriter.List[1].Count.ShouldBe(1);
            reportWriter.List[2].Format.ShouldBe("section.three");
            reportWriter.List[2].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(6));
            reportWriter.List[2].Count.ShouldBe(1);
        }

        [Fact]
        public void Profile_CreateAndDisposeSection_RepeatingTests()
        {
            var traceWriter = new StubTraceWriter();
            var reportWriter = new StubReportWriter();

            int index = 0;
            int[] milliseconds = new[] { 123, 45, 6 };

            Func<TimeSpan> getElapsed = () => TimeSpan.FromMilliseconds(milliseconds[index++]);

            var profile = new Profile(new StubFactory(() => new StubTimeMeasure(getElapsed), traceWriter, reportWriter));

            traceWriter.List.Count.ShouldBe(0);

            for (int i = 0; i < 3; i++)
            {
                using (profile.Section("section.{number}", i))
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

            profile.WriteReport();

            reportWriter.List.Count.ShouldBe(1);
            reportWriter.List[0].Format.ShouldBe("section.{number}");
            reportWriter.List[0].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(174));
            reportWriter.List[0].Count.ShouldBe(3);
        }

        [Fact]
        public void Profile_CreateAndDisposeSection_WithChilds()
        {
            var traceWriter = new StubTraceWriter();
            var reportWriter = new StubReportWriter();

            int index = 0;
            int[] milliseconds = new[] { 123, 123, 300 };

            Func<TimeSpan> getElapsed = () => TimeSpan.FromMilliseconds(milliseconds[index++]);

            var profile = new Profile(new StubFactory(() => new StubTimeMeasure(getElapsed), traceWriter, reportWriter));

            traceWriter.List.Count.ShouldBe(0);

            using (var section = profile.Section("outer"))
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

            profile.WriteReport();

            reportWriter.List.Count.ShouldBe(2);
            reportWriter.List[0].Format.ShouldBe("outer");
            reportWriter.List[0].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(300));
            reportWriter.List[0].Count.ShouldBe(1);
            reportWriter.List[1].Format.ShouldBe("outer -> inner");
            reportWriter.List[1].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(246));
            reportWriter.List[1].Count.ShouldBe(2);
        }
    }
}