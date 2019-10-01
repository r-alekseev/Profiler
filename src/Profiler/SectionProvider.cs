using System;
using System.Collections.Generic;
using System.Threading;

namespace Profiler
{
    public class SectionProvider : ISectionProvider
    {
        private readonly Func<ITimeMeasure> _getTimeMeasure;

        private readonly ITraceWriter _traceWriter;

        private readonly ThreadLocal<Dictionary<string, Section>> _local;

        public SectionProvider(Func<ITimeMeasure> getTimeMeasure, ITraceWriter traceWriter)
        {
            _traceWriter = traceWriter ?? throw new ArgumentNullException(nameof(traceWriter));
            _getTimeMeasure = getTimeMeasure ?? throw new ArgumentNullException(nameof(getTimeMeasure));

            _local = new ThreadLocal<Dictionary<string, Section>>(() => new Dictionary<string, Section>());
        }

        public ISection Section(string format, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                throw new ArgumentException("argument is null or whitespace", nameof(format));
            }

            var sections = _local.Value;

            if (sections.TryGetValue(format, out Section section))
            {
                if (section.InUse)
                {
                    throw new ArgumentException($"section '{format}' already in use", nameof(format));
                }
            }
            else
            {
                ITimeMeasure timeMeasure = _getTimeMeasure();
                section = new Section(this, timeMeasure, _traceWriter, format);
                sections.Add(format, section);
            }

            section.Enter(args);

            return section;
        }
    }
}
