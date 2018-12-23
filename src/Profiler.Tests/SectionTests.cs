using System;
using System.Collections.Generic;
using Shouldly;
using Xunit;

namespace Profiler.Tests
{
    class TestTimeMeasure : ITimeMeasure
    {
        private TimeSpan _elapsed;

        public TestTimeMeasure(TimeSpan elapsed)
        {
            _elapsed = elapsed;
        }

        public TimeSpan? Elapsed => _elapsed;
        public void Start() { }
        public void Stop() { }
    }

    class TestMeasureTrace : IMeasureTrace
    {
        public readonly List<(TimeSpan Elapsed, string Format, object[] Args)> List = new List<(TimeSpan, string, object[])>();

        public void Log(TimeSpan elapsed, string format, params object[] args)
        {
            List.Add((elapsed, format, args));
        }
    }

    public class SectionTests
    {
        [Fact]
        public void Section_CreateAndDispose_SingleTests()
        {
            var measureTrace = new TestMeasureTrace();

            Func<ITimeMeasure> getTimeMeasure = () => new TestTimeMeasure(TimeSpan.FromMilliseconds(123));

            var provider = new SectionProvider(getTimeMeasure, measureTrace);

            measureTrace.List.Count.ShouldBe(0);

            using (provider.Section("section"))
            {
                measureTrace.List.Count.ShouldBe(0);
            }

            measureTrace.List.Count.ShouldBe(1);
            measureTrace.List[0].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(123));
            measureTrace.List[0].Format.ShouldBe("section");
        }

        [Fact]
        public void Section_CreateAndDispose_SequentionalTests()
        {
            var measureTrace = new TestMeasureTrace();

            int index = 0;
            int[] seconds = new[] { 123, 45, 6 };
            Func<ITimeMeasure> getTimeMeasure = () => new TestTimeMeasure(TimeSpan.FromMilliseconds(seconds[index++]));

            var provider = new SectionProvider(getTimeMeasure, measureTrace);

            measureTrace.List.Count.ShouldBe(0);

            using (provider.Section("section.one"))
            {
                measureTrace.List.Count.ShouldBe(0);
            }

            measureTrace.List.Count.ShouldBe(1);
            measureTrace.List[0].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(123));
            measureTrace.List[0].Format.ShouldBe("section.one");
            measureTrace.List[0].Args.Length.ShouldBe(0);

            using (provider.Section("section.two"))
            {
                measureTrace.List.Count.ShouldBe(1);
            }

            measureTrace.List.Count.ShouldBe(2);
            measureTrace.List[0].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(123));
            measureTrace.List[0].Format.ShouldBe("section.one");
            measureTrace.List[0].Args.Length.ShouldBe(0);
            measureTrace.List[1].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(45));
            measureTrace.List[1].Format.ShouldBe("section.two");
            measureTrace.List[1].Args.Length.ShouldBe(0);

            using (provider.Section("section.three"))
            {
                measureTrace.List.Count.ShouldBe(2);
            }

            measureTrace.List.Count.ShouldBe(3);
            measureTrace.List[0].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(123));
            measureTrace.List[0].Format.ShouldBe("section.one");
            measureTrace.List[0].Args.Length.ShouldBe(0);
            measureTrace.List[1].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(45));
            measureTrace.List[1].Format.ShouldBe("section.two");
            measureTrace.List[1].Args.Length.ShouldBe(0);
            measureTrace.List[2].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(6));
            measureTrace.List[2].Format.ShouldBe("section.three");
            measureTrace.List[2].Args.Length.ShouldBe(0);
        }

        [Fact]
        public void Section_CreateAndDispose_RepeatingTests()
        {
            var measureTrace = new TestMeasureTrace();

            int index = 0;
            int[] seconds = new[] { 123, 45, 6 };
            Func<ITimeMeasure> getTimeMeasure = () => new TestTimeMeasure(TimeSpan.FromMilliseconds(seconds[index++]));

            var provider = new SectionProvider(getTimeMeasure, measureTrace);

            measureTrace.List.Count.ShouldBe(0);

            for (int i = 0; i < 3; i++)
            {
                using (provider.Section("section.{0}", i))
                {
                }
            }

            measureTrace.List.Count.ShouldBe(3);
            measureTrace.List[0].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(123));
            measureTrace.List[0].Format.ShouldBe("section.{0}");
            measureTrace.List[0].Args.Length.ShouldBe(1);
            measureTrace.List[0].Args[0].ShouldBe(0);

            measureTrace.List[1].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(45));
            measureTrace.List[1].Format.ShouldBe("section.{0}");
            measureTrace.List[1].Args.Length.ShouldBe(1);
            measureTrace.List[1].Args[0].ShouldBe(1);

            measureTrace.List[2].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(6));
            measureTrace.List[2].Format.ShouldBe("section.{0}");
            measureTrace.List[2].Args.Length.ShouldBe(1);
            measureTrace.List[2].Args[0].ShouldBe(2);
        }

        [Fact]
        public void Section_CreateAndDispose_WithChild_MetricsTests()
        {
            var measureTrace = new TestMeasureTrace();

            int i = 0;
            int[] seconds = new[] { 123, 45 };
            Func<ITimeMeasure> getTimeMeasure = () => new TestTimeMeasure(TimeSpan.FromMilliseconds(seconds[i++]));

            var provider = new SectionProvider(getTimeMeasure, measureTrace);

            measureTrace.List.Count.ShouldBe(0);

            using (provider.Section("outer"))
            {
                measureTrace.List.Count.ShouldBe(0);

                using (provider.Section("inner"))
                {
                    measureTrace.List.Count.ShouldBe(0);
                }

                measureTrace.List.Count.ShouldBe(1);
                measureTrace.List[0].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(45));
                measureTrace.List[0].Format.ShouldBe("inner");
            }

            measureTrace.List.Count.ShouldBe(2);
            measureTrace.List[0].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(45));
            measureTrace.List[0].Format.ShouldBe("inner");
            measureTrace.List[1].Elapsed.ShouldBe(TimeSpan.FromMilliseconds(123));
            measureTrace.List[1].Format.ShouldBe("outer");
        }

    }
}