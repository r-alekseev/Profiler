using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Profiler
{
    internal class Section : ISection, ISectionMetrics
    {
        private readonly Profiler _profiler;

        private int _count;

        private readonly ITimeMeasure _timeMeasure;
        private readonly ITraceWriter _traceWriter;

        private readonly string[] _chain;

        private object[] _args;

        private readonly object _inUseLocker = new object();
        private bool _inUse;

        private readonly ConcurrentQueue<Section> _queue;

        public Section(
            Profiler profiler,
            ITimeMeasure timeMeasure,
            ITraceWriter traceWriter,
            string[] chain,
            ConcurrentQueue<Section> queue)
        {
            _profiler = profiler ?? throw new ArgumentNullException(nameof(profiler));

            _timeMeasure = timeMeasure ?? throw new ArgumentNullException(nameof(timeMeasure));
            _traceWriter = traceWriter ?? throw new ArgumentNullException(nameof(traceWriter));

            _chain = chain ?? throw new ArgumentNullException(nameof(chain));

            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
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
                _traceWriter.Write(elapsed, _chain, _args);
                _queue.Enqueue(this);
            }
        }

        public void Dispose()
        {
            Exit();
        }

        ISection ISectionProvider.Section(string format, params object[] args)
        {
            string[] chain = CombineChain(format);

            var section = _profiler.GetOrCreateSection(chain, args);
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
