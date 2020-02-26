using UnityEditor.Experimental.AssetImporters;

namespace Unity.Platforms.Build
{
    [ScriptedImporter(1, new[] { BuildPipeline.AssetExtension })]
    sealed class BuildPipelineScriptedImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext context)
        {
            var asset = BuildPipeline.CreateInstance();
            if (BuildPipeline.DeserializeFromPath(asset, context.assetPath))
            {
                context.AddObjectToAsset("asset", asset/*, icon*/);
                context.SetMainObject(asset);
            }
        }
    }
}
