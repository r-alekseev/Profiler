using System;

namespace Profiler
{
    public class Section : ISection
    {
        private readonly ISectionProvider _provider;

        private readonly ITimeMeasure _timeMeasure;
        private readonly IMeasureTrace _measureTrace;

        private readonly string _format;
        private readonly object[] _args;

        internal Section(
            ISectionProvider provider, 
            ITimeMeasure timeMeasure, 
            IMeasureTrace measureTrace, 
            string format, 
            params object[] args)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));

            _timeMeasure = timeMeasure ?? throw new ArgumentNullException(nameof(timeMeasure));
            _measureTrace = measureTrace ?? throw new ArgumentNullException(nameof(measureTrace));

            if (string.IsNullOrWhiteSpace(format))
            {
                throw new ArgumentException("argument is null or whitespace", nameof(format));
            }

            _format = format;
            _args = args;

            _timeMeasure.Start();
        }

        public void Dispose()
        {
            _timeMeasure.Stop();

            _measureTrace.Log(_timeMeasure.Elapsed.Value, _format, _args);
        }

        ISection ISectionProvider.Section(string format, params object[] args)
        {
            var section = _provider.Section(format, args);
            return section;
        }
    }
}