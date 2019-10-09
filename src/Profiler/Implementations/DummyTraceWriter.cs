using System;
using System.Threading;

namespace Profiler
{
    public class DummyTraceWriter : ITraceWriter
    {
        private static readonly Lazy<DummyTraceWriter> _lazy =
            new Lazy<DummyTraceWriter>(
                valueFactory: () => new DummyTraceWriter(),
                mode: LazyThreadSafetyMode.ExecutionAndPublication);

        public static DummyTraceWriter Instance => _lazy.Value;

        private DummyTraceWriter() { }

        public void Write(TimeSpan elapsed, string[] chain, params object[] args) { }
    }
}
