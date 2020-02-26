using Unity.Properties;
using UnityEngine;

namespace Unity.Platforms.Build.Common
{
    [FormerlySerializedAs("Unity.Build.Common.GraphicsSettings, Unity.Build.Common")]
    public sealed class GraphicsSettings : IBuildComponent
    {
        public ColorSpace ColorSpace = ColorSpace.Uninitialized;
    }
}
