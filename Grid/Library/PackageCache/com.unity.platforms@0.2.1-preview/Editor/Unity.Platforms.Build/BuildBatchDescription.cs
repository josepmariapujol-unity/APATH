using System;
using System.ComponentModel;
using UnityEngine.Events;

namespace Unity.Platforms.Build
{
    public struct BuildBatchItem
    {
        public BuildConfiguration BuildConfiguration;
        public Action<BuildContext> Mutator;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("BuildSettings has been renamed to BuildConfiguration. (RemovedAfter 2020-04-13) (UnityUpgradable) -> BuildConfiguration")]
        public BuildConfiguration BuildSettings;
    }

    public struct BuildBatchDescription
    {
        public BuildBatchItem[] BuildItems;
        public UnityAction<BuildPipelineResult[]> OnBuildCompleted;
    }
}
