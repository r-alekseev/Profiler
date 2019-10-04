using System;

namespace Profiler
{
    public interface ISection : IDisposable
    {
        ISection Section(string format, params object[] args);
        void Free();
    }
}
