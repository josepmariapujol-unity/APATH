using Newtonsoft.Json.Linq;

namespace Unity.Platforms.Build.DotsRuntime
{
    public interface IDotsRuntimeBuildModifier : IBuildComponent
    {
        void Modify(JObject settingsJObject);
    }
}
