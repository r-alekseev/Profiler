using System;
using System.Collections.Generic;
using Shouldly;
using Xunit;

namespace Profiler.Tests
{
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
        public readonly List<(TimeSpan Elapsed, int Count, string Format)> List = new List<(TimeSpan, int, string)>();

        public void Write(int threadId, TimeSpan elapsed, int count, string[] chain)
        {
            List.Add((elapsed, count, string.Join(" -> ", chain)));
        }
    }

    public class SectionProviderTests
    {
        [Fact]
        public void SectionProvider_Create_NullTimeMeasure_ShouldThrowArgumentNullException()
        {
            Should.Throw<ArgumentNullException>(() =>
            {
                new SectionProvider(null, new StubTraceWriter(), new StubReportWriter());
            });
        }

        [Fact]
        public void SectionProvider_Create_NullTraceWriter_ShouldThrowArgumentNullException()
        {
            Should.Throw<ArgumentNullException>(() =>
            {
                new SectionProvider(() => new StubTimeMeasure(() => TimeSpan.Zero), null, new StubReportWriter());
            });
        }

        [Fact]
        public void SectionProvider_Create_NullReportWriter_ShouldThrowArgumentNullException()
        {
            Should.Throw<ArgumentNullException>(() =>
            {
                new SectionProvider(() => new StubTimeMeasure(() => TimeSpan.Zero), new StubTraceWriter(), null);
            });
        }

        [Fact]
        public void SectionProvider_CreateSection_NullFormat_ShouldThrowArgumentException()
        {
            var sectionProvider = new SectionProvider(
                () => new StubTimeMeasure(() => TimeSpan.Zero), 
                new StubTraceWriter(),
                new StubReportWriter());

            Should.Throw<ArgumentException>(() =>
            {
                sectionProvider.Section(null);
            });
        }

        [Fact]
        public void SectionProvider_CreateSection_EmptyFormat_ShouldThrowArgumentException()
        {
            var sectionProvider = new SectionProvider(
                () => new StubTimeMeasure(() => TimeSpan.Zero),
                new StubTraceWriter(),
                new StubReportWriter());

            Should.Throw<ArgumentException>(() =>
            {
                sectionProvider.Section(string.Empty);
            });
        }

        [Fact]
        public void SectionProvider_CreateSection_AlreadyInUse_ShouldThrowArgumentException()
        {
            var sectionProvider = new SectionProvider(
                () => new StubTimeMeasure(() => TimeSpan.Zero),
                new StubTraceWriter(),
                new StubReportWriter());

            // first usage of 'test'
            using (sectionProvider.Section("test"))
            {
                Should.Throw<ArgumentException>(() =>
                {
                    // already in use
                    using (sectionProvider.Section("test"))
                    {
                    }
                });
            }
        }

        [Fact]
        public void SectionProvider_CreateSection_ReuseAfterDispose_ShouldNotThrowExceptions()
        {
            var sectionProvider = new SectionProvider(
                () => new StubTimeMeasure(() => TimeSpan.Zero),
                new StubTraceWriter(),
                new StubReportWriter());

            // first usage of 'test'
            using (sectionProvider.Section("test"))
            {
            }   // freeing 'test'

            // second usage of 'test'
            using (sectionProvider.Section("test"))
            {
            }
        }

        [Fact]
        public void SectionProvider_CreateSection_UseSimilarNameForInnerSection_ShouldNotThrowExceptions()
        {
            var sectionProvider = new SectionProvider(
                () => new StubTimeMeasure(() => TimeSpan.Zero),
                new StubTraceWriter(),
                new StubReportWriter());

            // usage of 'test'
            using (var section = sectionProvider.Section("test"))
            {
                // usage of 'test -> test'
                using (section.Section("test"))
                {
                }
            }
        }

        [Fact]
        public void SectionProvider_CreateAndDisposeSection_SingleTests()
        {
            var traceWriter = new StubTraceWriter();
            var reportWriter = new StubReportWriter();

            Func<TimeSpan> getElapsed = () => TimeSpan.FromMilliseconds(123);
            Func<ITimeMeasure> getTimeMeasure = () => new StubTimeMeasure(getElapsed);

            var provider = new SectionProvider(getTimeMeasure, traceWriter, reportWriter);

            traceWriter.List.Count.ShouldBe(0);

            using (provider.Section("section"))
            {
                traceWriter.List.Count.ShouldBe(0);
            }

            traceWriter.List.Count.ShouldBe(1);
            traceWriter.List[0].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(123));
            traceWriter.List[0].Format.ShouldBe("section");

            provider.WriteReport();

            reportWriter.List.Count.ShouldBe(1);
            reportWriter.List[0].Format.ShouldBe("section");
            reportWriter.List[0].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(123));
            reportWriter.List[0].Count.ShouldBe(1);
        }

        [Fact]
        public void SectionProvider_CreateAndDisposeSection_SequentionalTests()
        {
            var traceWriter = new StubTraceWriter();
            var reportWriter = new StubReportWriter();

            int index = 0;
            int[] milliseconds = new[] { 123, 45, 6 };

            Func<TimeSpan> getElapsed = () => TimeSpan.FromMilliseconds(milliseconds[index++]);
            Func<ITimeMeasure> getTimeMeasure = () => new StubTimeMeasure(getElapsed);

            var provider = new SectionProvider(getTimeMeasure, traceWriter, reportWriter);

            traceWriter.List.Count.ShouldBe(0);

            using (provider.Section("section.one"))
            {
                traceWriter.List.Count.ShouldBe(0);
            }

            traceWriter.List.Count.ShouldBe(1);
            traceWriter.List[0].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(123));
            traceWriter.List[0].Format.ShouldBe("section.one");
            traceWriter.List[0].Args.Length.ShouldBe(0);

            using (provider.Section("section.two"))
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

            using (provider.Section("section.three"))
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

            provider.WriteReport();

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
        public void SectionProvider_CreateAndDisposeSection_RepeatingTests()
        {
            var traceWriter = new StubTraceWriter();
            var reportWriter = new StubReportWriter();

            int index = 0;
            int[] milliseconds = new[] { 123, 45, 6 };

            Func<TimeSpan> getElapsed = () => TimeSpan.FromMilliseconds(milliseconds[index++]);
            Func<ITimeMeasure> getTimeMeasure = () => new StubTimeMeasure(getElapsed);

            var provider = new SectionProvider(getTimeMeasure, traceWriter, reportWriter);

            traceWriter.List.Count.ShouldBe(0);

            for (int i = 0; i < 3; i++)
            {
                using (provider.Section("section.{number}", i))
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

            provider.WriteReport();

            reportWriter.List.Count.ShouldBe(1);
            reportWriter.List[0].Format.ShouldBe("section.{number}");
            reportWriter.List[0].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(174));
            reportWriter.List[0].Count.ShouldBe(3);
        }

        [Fact]
        public void SectionProvider_CreateAndDisposeSection_WithChilds()
        {
            var traceWriter = new StubTraceWriter();
            var reportWriter = new StubReportWriter();

            int index = 0;
            int[] milliseconds = new[] { 123, 123, 300 };

            Func<TimeSpan> getElapsed = () => TimeSpan.FromMilliseconds(milliseconds[index++]);
            Func<ITimeMeasure> getTimeMeasure = () => new StubTimeMeasure(getElapsed);
            var provider = new SectionProvider(getTimeMeasure, traceWriter, reportWriter);

            traceWriter.List.Count.ShouldBe(0);

            using (var section = provider.Section("outer"))
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

            provider.WriteReport();

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