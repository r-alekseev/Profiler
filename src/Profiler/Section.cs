using System;
using System.Threading;

namespace Profiler
{
    internal class Section : ISection
    {
        private readonly int _threadId = Thread.CurrentThread.ManagedThreadId;

        private readonly SectionProvider _sectionProvider;

        private int _count;

        private readonly ITimeMeasure _timeMeasure;
        private readonly ITraceWriter _traceWriter;

        private readonly string[] _chain;

        private object[] _args;

        private bool _inUse;

        public Section(
            SectionProvider sectionProvider,
            ITimeMeasure timeMeasure,
            ITraceWriter traceWriter,
            string[] chain)
        {
            _sectionProvider = sectionProvider ?? throw new ArgumentNullException(nameof(sectionProvider));

            _timeMeasure = timeMeasure ?? throw new ArgumentNullException(nameof(timeMeasure));
            _traceWriter = traceWriter ?? throw new ArgumentNullException(nameof(traceWriter));

            _chain = chain ?? throw new ArgumentNullException(nameof(chain));
        }

        public void Enter(object[] args)
        {
            _args = args;
            _inUse = true;
            _timeMeasure.Start();
        }

        public bool InUse => _inUse;
        public string[] Chain => _chain;
        public ITimeMeasure TimeMeasure => _timeMeasure;
        public int Count => _count;
        public int ThreadId => _threadId;

        public void Free()
        {
            if (_inUse)
            {
                _count += 1;
                TimeSpan elapsed = _timeMeasure.Pause();
                _traceWriter.Write(_threadId, elapsed, _chain, _args);
                _inUse = false;
            }
        }

        public void Dispose()
        {
            Free();
        }

        ISection ISectionProvider.Section(string format, params object[] args)
        {
            string[] chain = CombineChain(format);

            var section = _sectionProvider.Section(chain, args);
            return section;
        }

        private string[] CombineChain(string format)
        {
            string[] formats = new string[_chain.Length + 1];
            for (int i = 0; i < _chain.Length; i++)
            {
                formats[i] = _chain[i];
            }
            formats[_chain.Length] = format;
            return formats;
        }
    }
}
