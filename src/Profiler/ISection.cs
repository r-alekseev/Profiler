using System;

namespace Profiler
{
    public interface ISection : ISectionProvider, IDisposable
    {
        void Free();
    }
}
