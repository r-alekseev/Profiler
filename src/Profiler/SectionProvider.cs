using System;

namespace Profiler
{
    public class SectionProvider : ISectionProvider
    {
        private readonly Func<ITimeMeasure> _getTimeMeasure;
        private readonly IMeasureTrace _measureTrace;

        public SectionProvider(Func<ITimeMeasure> getTimeMeasure, IMeasureTrace measureTrace)
        {
            _getTimeMeasure = getTimeMeasure ?? throw new ArgumentNullException(nameof(getTimeMeasure));
            _measureTrace = measureTrace ?? throw new ArgumentNullException(nameof(measureTrace));
        }

        public ISection Section(string format, params object[] args)
        {
            var timeMeasure = _getTimeMeasure();
            var section = new Section(this, timeMeasure, _measureTrace, format, args);
            return section;
        }
    }
}
