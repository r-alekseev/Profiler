using System;
using System.Threading;

namespace Profiler
{
    internal class Section : ISection, ISectionMetrics
    {
        private readonly int _threadId = Thread.CurrentThread.ManagedThreadId;

        private readonly Profiler _sectionProvider;

        private int _count;

        private readonly ITimeMeasure _timeMeasure;
        private readonly ITraceWriter _traceWriter;

        private readonly string[] _chain;

        private object[] _args;

        private readonly object _inUseLocker = new object();
        private bool _inUse;

        public Section(
            Profiler sectionProvider,
            ITimeMeasure timeMeasure,
            ITraceWriter traceWriter,
            string[] chain)
        {
            _sectionProvider = sectionProvider ?? throw new ArgumentNullException(nameof(sectionProvider));

            _timeMeasure = timeMeasure ?? throw new ArgumentNullException(nameof(timeMeasure));
            _traceWriter = traceWriter ?? throw new ArgumentNullException(nameof(traceWriter));

            _chain = chain ?? throw new ArgumentNullException(nameof(chain));
        }

        internal void Enter(object[] args)
        {
            _args = args;
            _inUse = true;
            _timeMeasure.Start();
        }

        public bool InUse => _inUse;
        public string[] Chain => _chain;
        public TimeSpan Elapsed => _timeMeasure.Total;
        public int Count => _count;
        public int ThreadId => _threadId;

        internal void Exit()
        {
            bool inUse;
            lock (_inUseLocker)
            {
                inUse = _inUse;
                _inUse = false;
            }

            if (inUse)
            {
                _count += 1;
                TimeSpan elapsed = _timeMeasure.Pause();
                _traceWriter.Write(_threadId, elapsed, _chain, _args);
            }
        }

        public void Dispose()
        {
            Exit();
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
