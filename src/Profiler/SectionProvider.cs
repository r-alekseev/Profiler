using System;
using System.Collections.Generic;
using System.Threading;

namespace Profiler
{
    public class SectionProvider : ISectionProvider
    {
        private readonly IFactory _factory;

        private readonly ITraceWriter _traceWriter;
        private readonly IReportWriter _reportWriter;

        private readonly ThreadLocal<Dictionary<CollectionKey, Section>> _local;

        public SectionProvider(IFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));

            _traceWriter = _factory.CreateTraceWriter() ?? throw new ArgumentException($"factory returns null {nameof(ITraceWriter)}", nameof(factory));
            _reportWriter = _factory.CreateReportWriter() ?? throw new ArgumentException($"factory returns null {nameof(IReportWriter)}", nameof(factory)); ;

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
                ITimeMeasure timeMeasure = _factory.CreateTimeMeasure() ?? throw new InvalidOperationException($"factory returns null {nameof(IReportWriter)}");
                section = new Section(this, timeMeasure, _traceWriter, chain);
                sections.Add(key, section);

                _reportWriter.Add(section);
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
            _reportWriter.Write();
        }
    }
}
