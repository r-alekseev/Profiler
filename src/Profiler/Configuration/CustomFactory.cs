using System;

namespace Profiler
{
    public class CustomFactory : IFactory
    {
        private readonly Func<ITimeMeasure> _createTimeMeasure;
        private readonly Func<ITraceWriter> _createTraceWriter;
        private readonly Func<IReportWriter> _createReportWriter;

        public CustomFactory(ICustomFactorySettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            _createTimeMeasure = settings.CreateTimeMeasure ?? (() => new StopwatchTimeMeasure());
            _createTraceWriter = settings.CreateTraceWriter ?? (() => DummyTraceWriter.Instance);
            _createReportWriter = settings.CreateReportWriter ?? (() => DummyReportWriter.Instance);
        }

        public ITimeMeasure CreateTimeMeasure() => _createTimeMeasure();
        public ITraceWriter CreateTraceWriter() => _createTraceWriter();
        public IReportWriter CreateReportWriter() => _createReportWriter();
    }
}
