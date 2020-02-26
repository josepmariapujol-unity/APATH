using System;
using System.ComponentModel;

namespace Unity.Platforms.Build
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("BuildSettings has been renamed to BuildConfiguration. (RemovedAfter 2020-04-13) (UnityUpgradable) -> BuildConfiguration")]
    public sealed class BuildSettings
    {
        public const string AssetExtension = ".buildsettings";
    }
}
