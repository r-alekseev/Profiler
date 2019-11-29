namespace Profiler
{
    public interface IFactory
    {
        IReportWriter CreateReportWriter();
        ITraceWriter CreateTraceWriter();
        ITimeMeasure CreateTimeMeasure();
    }
}
