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

        private readonly object _locker = new object();
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
            lock (_locker)
            {
                if (_inUse)
                {
                    throw new Exception("can't repeat enter to active section");
                }

                _inUse = true;
                _args = args;
                _timeMeasure.Start();
            }
        }

        public string[] Chain => _chain;
        public TimeSpan Elapsed => _timeMeasure.Total;
        public int Count => _count;

        internal void Exit()
        {
            lock (_locker)
            {
                if (_inUse)
                {
                    _count += 1;
                    TimeSpan elapsed = _timeMeasure.Pause();
                    _traceWriter.Write(elapsed, _chain, _args);
                    _queue.Enqueue(this);
                    _inUse = false;
                }
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
