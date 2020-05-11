using Xunit;
using System.Threading.Tasks;

namespace Profiler.FuzzingTests
{
    public partial class ProfilerTests
    {
        [Fact]
        public async Task Profiler_CreateSection_EnterSectionsSimultaneously_ExitAfterAwait_ShouldNotThrowSectionInUseException()
        {
            var profiler = new ProfilerConfiguration()
                .CreateProfiler();

            int count = 1000;
            var tasks = new Task[count];

            for (int i = 0; i < count; i++)
            {
                tasks[i] = await Task.Factory.StartNew(async () =>
                {
                    int j = i;
                    var section = profiler.Section("section");

                    //Thread.Sleep(1000); - okay

                    await Task.Delay(1000).ConfigureAwait(false); // -- error

                    section.Dispose();
                });
            }

            await Task.WhenAll(tasks);
        }
    }
}
