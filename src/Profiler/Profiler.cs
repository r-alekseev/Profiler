using System;
using System.Collections.Concurrent;

namespace Profiler
{
    public class Profiler : IProfiler
    {
        private readonly IFactory _factory;

        private readonly ITraceWriter _traceWriter;
        private readonly IReportWriter _reportWriter;

        private readonly ConcurrentDictionary<string[], ConcurrentQueue<Section>> _pool;

        public Profiler(IFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));

            _traceWriter = _factory.CreateTraceWriter() ?? throw new ArgumentException($"factory returns null {nameof(ITraceWriter)}", nameof(factory));
            _reportWriter = _factory.CreateReportWriter() ?? throw new ArgumentException($"factory returns null {nameof(IReportWriter)}", nameof(factory)); ;

            _pool = new ConcurrentDictionary<string[], ConcurrentQueue<Section>>(
                comparer: new ChainEqualityComparer());
        }

        internal ISection GetOrCreateSection(string[] chain, params object[] args)
        {
            if (chain == null) throw new ArgumentNullException(nameof(chain));
            if (chain.Length == 0) throw new ArgumentException("empty collection", nameof(chain));

            for (int i = 0; i < chain.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(chain[i]))
                    throw new ArgumentException("at least one of chain items is null or whitespace", nameof(chain));
            }

            var queue = _pool.GetOrAdd(chain, k => new ConcurrentQueue<Section>());

            if (!queue.TryDequeue(out Section section))
            {
                ITimeMeasure timeMeasure = _factory.CreateTimeMeasure() ?? throw new InvalidOperationException($"factory returns null {nameof(IReportWriter)}");
                section = new Section(this, timeMeasure, _traceWriter, chain, queue);
                _reportWriter.Add(section);
            }

            section.Enter(args);

            return section;
        }

        public ISection Section(string format, params object[] args)
        {
            return GetOrCreateSection(
                chain: new[] { format }, 
                args: args);
        }

        public void WriteReport()
        {
            _reportWriter.Write();
        }
    }
}
