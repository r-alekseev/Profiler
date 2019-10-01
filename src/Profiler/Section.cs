﻿using System;

namespace Profiler
{
    internal class Section : ISection
    {
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

        public void Dispose()
        {
            TimeSpan elapsed = _timeMeasure.Pause();
            _traceWriter.Write(elapsed, _format, _args);
            _inUse = false;
        }

        ISection ISectionProvider.Section(string format, params object[] args)
        {
            var section = _sectionProvider.Section($"{_format} -> {format}", args);
            return section;
        }
    }
}
