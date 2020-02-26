using System;

namespace Unity.Platforms.Build
{
    public interface IRunInstance : IDisposable
    {
        bool IsRunning { get; }
    }
}
