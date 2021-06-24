using Summer.Base;
using Summer.Debugger.Window.Model;
using Summer.Resource;
using Spring.Core;
using Spring.Logger;
using Spring.Util;
using UnityEngine;
using UnityEngine.Rendering;

namespace Summer.Debugger.Window
{
    public sealed class EnvironmentInformationWindow : ScrollableDebuggerWindowBase
    {
        private BaseComponent baseComponent = null;
        private ResourceComponent resourceComponent = null;

        public override void Initialize(params object[] args)
        {
            baseComponent = SpringContext.GetBean<BaseComponent>();
            if (baseComponent == null)
            {
                Log.Fatal("Base component is invalid.");
                return;
            }

            resourceComponent = SpringContext.GetBean<ResourceComponent>();
            if (resourceComponent == null)
            {
                Log.Fatal("Resource component is invalid.");
                return;
            }
        }

        protected override void OnDrawScrollableWindow()
        {
            GUILayout.Label("<b>Environment Information</b>");
            GUILayout.BeginVertical("box");
            {
                DrawItem("Product Name", Application.productName);
                DrawItem("Company Name", Application.companyName);
                DrawItem("Game Identifier", Application.identifier);
                DrawItem("Resource Version", baseComponent.editorResourceMode
                    ? "Unavailable in editor resource mode"
                    : (string.IsNullOrEmpty(SpringContext.GetBean<IResourceManager>().ApplicableGameVersion)
                        ? "Unknown"
                        : StringUtils.Format("{} ({})", SpringContext.GetBean<IResourceManager>().ApplicableGameVersion, SpringContext.GetBean<IResourceManager>().InternalResourceVersion.ToString())));
                DrawItem("Application Version", Application.version);
                DrawItem("Unity Version", Application.unityVersion);
                DrawItem("Platform", Application.platform.ToString());
                DrawItem("System Language", Application.systemLanguage.ToString());
                DrawItem("Cloud Project Id", Application.cloudProjectId);
                DrawItem("Build Guid", Application.buildGUID);
                DrawItem("Target Frame Rate", Application.targetFrameRate.ToString());
                DrawItem("Internet Reachability", Application.internetReachability.ToString());
                DrawItem("Background Loading Priority", Application.backgroundLoadingPriority.ToString());
                DrawItem("Is Playing", Application.isPlaying.ToString());
                DrawItem("Splash Screen Is Finished", SplashScreen.isFinished.ToString());
                DrawItem("Run In Background", Application.runInBackground.ToString());
                DrawItem("Install Name", Application.installerName);
                DrawItem("Install Mode", Application.installMode.ToString());
                DrawItem("Sandbox Type", Application.sandboxType.ToString());
                DrawItem("Is Mobile Platform", Application.isMobilePlatform.ToString());
                DrawItem("Is Console Platform", Application.isConsolePlatform.ToString());
                DrawItem("Is Editor", Application.isEditor.ToString());
                DrawItem("Is Focused", Application.isFocused.ToString());
                DrawItem("Is Batch Mode", Application.isBatchMode.ToString());
            }
            GUILayout.EndVertical();
        }
    }
}