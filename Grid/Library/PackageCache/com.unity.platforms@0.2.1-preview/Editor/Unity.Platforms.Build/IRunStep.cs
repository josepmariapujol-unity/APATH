using System;
using System.ComponentModel;

namespace Unity.Platforms.Build
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Run steps must derive from RunStep instead of IRunStep. (RemovedAfter 2020-04-13) (UnityUpgradable) -> RunStep")]
    public interface IRunStep
    {
    }
}
