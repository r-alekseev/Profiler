using System;
using System.Diagnostics;
using System.Threading;

namespace Profiler
{
    public class DebugTraceWriter : ITraceWriter
    {
        private static readonly Lazy<DebugTraceWriter> _lazy =
            new Lazy<DebugTraceWriter>(
                valueFactory: () => new DebugTraceWriter(),
                mode: LazyThreadSafetyMode.ExecutionAndPublication);

        public static DebugTraceWriter Instance => _lazy.Value;

        private DebugTraceWriter() { }

        public void Write(TimeSpan elapsed, string[] chain, params object[] args)
        {
            Debug.Write($"[{elapsed.TotalMilliseconds} ms] {string.Join(" -> ", chain)}");
            if (args.Length > 0)
            {
                Debug.Write($": {string.Join(", ", args)}");
            }
            Debug.Write(Environment.NewLine);
        }
    }
}
