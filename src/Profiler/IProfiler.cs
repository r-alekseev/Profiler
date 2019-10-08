namespace Profiler
{
    public interface IProfiler : ISectionProvider
    {
        void WriteReport();
    }
}
