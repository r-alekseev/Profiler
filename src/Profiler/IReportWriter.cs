namespace Profiler
{
    public interface IReportWriter
    {
        void Add(ISectionMetrics metrics);
        void Write();
    }
}
