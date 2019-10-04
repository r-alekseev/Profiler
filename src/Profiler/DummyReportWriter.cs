using Profiler;
using System;
using System.Threading;

namespace Profiler
{
    public class DummyReportWriter : IReportWriter
    {
        private static readonly Lazy<DummyReportWriter> _lazy =
            new Lazy<DummyReportWriter>(
                valueFactory: () => new DummyReportWriter(),
                mode: LazyThreadSafetyMode.ExecutionAndPublication);

        public static DummyReportWriter Instance => _lazy.Value;

        private DummyReportWriter() { }

        public void Add(ISectionMetrics metrics) { }

        public void Write() { }
    }
}