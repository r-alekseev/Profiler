using Xunit;

namespace Profiler.Tests
{
    public class ProfilerTests
    {
        private ISectionProvider Stub()
        {
            return new StubSectionProvider();
        }

        [Fact]
        public void Profiler_Section_Sequential_Tests()
        {
            var provider = Stub();

            using (provider.Section("section.one"))
            {
            }

            using (provider.Section("section.two"))
            {
            }

            using (provider.Section("section.three"))
            {
            }

            // log:
            //  section.one
            //  section.two
            //  section.three

            // metrics:
            //  section.one     : 1
            //  section.two     : 1
            //  section.three   : 1
        }

        [Fact]
        public void Profiler_Section_Repeating_Tests()
        {
            var provider = Stub();

            for (int i = 0; i < 3; i++)
            {
                using (provider.Section("section.{0}", i))
                {
                }
            }

            // log:
            //  section.0
            //  section.1
            //  section.2

            // metrics:
            //  section.{0}     : 3
        }


        [Fact]
        public void Profiler_Section_Childs_Tests()
        {
            var provider = Stub();

            using (var section = provider.Section("section.one"))
            {
                using (section.Section("child.one"))
                {
                }
            }

            // log:
            //  section.one -> child.one
            //  section.one

            // metrics:
            //  section.one                 : 1
            //  section.one -> child.one    : 1
        }

        [Fact]
        public void Profiler_Section_Child_Passing_Tests()
        {
            var provider = Stub();

            void Inner(ISection section, int i)
            {
                using (section.Section("child.{0}", i))
                {
                }
            }

            using (var section = provider.Section("section.one"))
            {
                Inner(section, 0);
                Inner(section, 1);
                Inner(section, 2);
            }

            // log:
            //  section.one -> child.0
            //  section.one -> child.1
            //  section.one -> child.2
            //  section.one

            // metrics:
            //  section.one                 : 1
            //  section.one -> child.{0}    : 3
        }

        class StubSectionProvider : ISectionProvider
        {
            class StubSection : ISection
            {
                public void Dispose()
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
        }
    }
}
