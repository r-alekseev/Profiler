using System;
using System.Collections.Generic;
using System.Text;

namespace Profiler
{
    public interface IFactory
    {
        IReportWriter CreateReportWriter();
        ITraceWriter CreateTraceWriter();
        ITimeMeasure CreateTimeMeasure();
    }
}
