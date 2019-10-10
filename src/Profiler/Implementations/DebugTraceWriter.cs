using System;
using System.Diagnostics;

namespace Profiler
{
    public class DebugTraceWriter : ITraceWriter
    {
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
