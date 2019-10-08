using Xunit;

namespace Profiler.Tests
{
    public class ProfilerTests
    {
        private IProfiler Stub()
        {
            return new StubProfile();
        }

        [Fact]
        public void Profiler_Section_Sequential_Tests()
        {
            var profiler = Stub();

            using (profiler.Section("section.one"))
            {
                // delay 1 ms
            }

            using (profiler.Section("section.two"))
            {
                // delay 1 ms
            }

            using (profiler.Section("section.three"))
            {
                // delay 1 ms
            }

            // trace:
            //  section.one     : 1 ms
            //  section.two     : 1 ms
            //  section.three   : 1 ms

            // metrics:
            //  section.one     : 1 ms
            //  section.two     : 1 ms
            //  section.three   : 1 ms
        }

        [Fact]
        public void Profiler_Section_Repeating_Tests()
        {
            var profiler = Stub();

            for (int i = 0; i < 3; i++)
            {
                using (profiler.Section("section.{number}", i))
                {
                    // delay 1 ms
                }
            }

            // trace:
            //  section.0           : 1 ms
            //  section.1           : 1 ms
            //  section.2           : 1 ms

            // metrics:
            //  section.{number}    : 3 ms
        }


        [Fact]
        public void Profiler_Section_Childs_Tests()
        {
            var profiler = Stub();

            using (var section = profiler.Section("section.one"))
            {
                using (section.Section("child.one"))
                {
                    // delay 1 ms
                }

                // delay 1 ms
            }

            // trace:
            //  section -> child        : 1 ms
            //  section                 : 2 ms

            // metrics:
            //  section                 : 2 ms
            //  section -> child        : 1 ms
        }

        [Fact]
        public void Profiler_Section_Child_Passing_Tests()
        {
            var profiler = Stub();

            void Inner(ISection section, int i)
            {
                using (section.Section("child.{number}", i))
                {
                    // delay 1 ms
                }
            }

            using (var section = profiler.Section("section.one"))
            {
                Inner(section, 0);
                Inner(section, 1);
                Inner(section, 2);

                // delay 1 ms
            }

            // trace:
            //  section -> child.0          : 1 ms
            //  section -> child.1          : 1 ms
            //  section -> child.2          : 1 ms
            //  section                     : 4 ms

            // metrics:
            //  section                     : 4 ms
            //  section -> child.{number}   : 3 ms
        }

        class StubProfile : IProfiler
        {
            class StubSection : ISection
            {
                public void Dispose()
                {
                }

                public void Exit()
                {
                }

                public ISection Section(string format, params object[] args)
                {
                    return new StubSection();
                }
            }

            public ISection Section(string format, params object[] args)
            {
                return new StubSection();
            }

            public void WriteReport()
            {
            }
        }
    }
}
