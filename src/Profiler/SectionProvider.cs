using System;
using System.Collections.Generic;
using System.Threading;

namespace Profiler
{
    public class SectionProvider : ISectionProvider
    {
        private readonly Func<ITimeMeasure> _getTimeMeasure;

        private readonly ITraceWriter _traceWriter;
        private readonly IReportWriter _reportWriter;

        private readonly object _globalLocker = new object();
        private readonly List<Section> _global;

        private readonly ThreadLocal<Dictionary<CollectionKey, Section>> _local;

        public SectionProvider(Func<ITimeMeasure> getTimeMeasure, ITraceWriter traceWriter, IReportWriter reportWriter)
        {
            _getTimeMeasure = getTimeMeasure ?? throw new ArgumentNullException(nameof(getTimeMeasure));

            _traceWriter = traceWriter ?? throw new ArgumentNullException(nameof(traceWriter));
            _reportWriter = reportWriter ?? throw new ArgumentNullException(nameof(reportWriter));

            _global = new List<Section>();
            _local = new ThreadLocal<Dictionary<CollectionKey, Section>>(() => new Dictionary<CollectionKey, Section>());
        }

        internal ISection Section(string[] chain, params object[] args)
        {
            if (chain == null) throw new ArgumentNullException(nameof(chain));
            if (chain.Length == 0) throw new ArgumentException("empty collection", nameof(chain));

            for (int i = 0; i < chain.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(chain[i]))
                    throw new ArgumentException("at least one of chain items is null or whitespace", nameof(chain));
            }

            var key = new CollectionKey(chain);

            var sections = _local.Value;

            if (sections.TryGetValue(key, out Section section))
            {
                if (section.InUse)
                {
                    throw new ArgumentException($"section '{string.Format(" -> ", chain)}' already in use", nameof(chain));
                }
            }
            else
            {
                ITimeMeasure timeMeasure = _getTimeMeasure();
                section = new Section(this, timeMeasure, _traceWriter, chain);
                sections.Add(key, section);

                lock (_globalLocker)
                {
                    _global.Add(section);
                }
            }

            section.Enter(args);

            return section;
        }

        public ISection Section(string format, params object[] args)
        {
            return Section(
                chain: new[] { format }, 
                args: args);
        }

        public void WriteReport()
        {
            Section[] sections;
            lock (_globalLocker)
            {
                sections = _global.ToArray();
            }

            foreach (var section in sections)
            {
                _reportWriter.Write(
                    threadId: section.ThreadId,
                    elapsed: section.TimeMeasure.Total,
                    count: section.Count,
                    chain: section.Chain);
            }
        }
    }
}
