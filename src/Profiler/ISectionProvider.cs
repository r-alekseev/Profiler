using System.Collections.Generic;

namespace Profiler
{
    public interface ISectionProvider
    {
        ISection Section(string format, params object[] args);
    }
}
