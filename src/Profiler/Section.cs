using System;
using System.Threading;

namespace Profiler
{
    internal class Section : ISection
    {
        private readonly int _threadId = Thread.CurrentThread.ManagedThreadId;

        private readonly ISectionProvider _sectionProvider;

        private readonly ITimeMeasure _timeMeasure;
        private readonly ITraceWriter _traceWriter;

        private readonly string _format;

        private object[] _args;

        private bool _inUse;

        public Section(
            ISectionProvider sectionProvider,
            ITimeMeasure timeMeasure,
            ITraceWriter traceWriter,
            string format)
        {
            _sectionProvider = sectionProvider ?? throw new ArgumentNullException(nameof(sectionProvider));

            _timeMeasure = timeMeasure ?? throw new ArgumentNullException(nameof(timeMeasure));
            _traceWriter = traceWriter ?? throw new ArgumentNullException(nameof(traceWriter));

            if (string.IsNullOrWhiteSpace(format))
            {
                throw new ArgumentException("argument is null or whitespace", nameof(format));
            }

            _format = format;
        }

        public void Enter(object[] args)
        {
            _args = args;
            _inUse = true;
            _timeMeasure.Start();
        }

        public bool InUse => _inUse;
        public string Format => _format;
        public ITimeMeasure TimeMeasure => _timeMeasure;
        public int ThreadId => _threadId;

        public void Free()
        {
            TimeSpan elapsed = _timeMeasure.Pause();
            _traceWriter.Write(_threadId, elapsed, _format, _args);
            _inUse = false;
        }

        public void Dispose()
        {
            Free();
        }

        ISection ISectionProvider.Section(string format, params object[] args)
        {
            var section = _sectionProvider.Section($"{_format} -> {format}", args);
            return section;
        }
    }
}
