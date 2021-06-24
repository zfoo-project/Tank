using Summer.Debugger.Window.Model;
using UnityEngine;

namespace Summer.Debugger.Window
{
    public sealed class PathInformationWindow : ScrollableDebuggerWindowBase
    {
        protected override void OnDrawScrollableWindow()
        {
            GUILayout.Label("<b>Path Information</b>");
            GUILayout.BeginVertical("box");
            {
                DrawItem("Data Path", Application.dataPath);
                DrawItem("Persistent Data Path", Application.persistentDataPath);
                DrawItem("Streaming Assets Path", Application.streamingAssetsPath);
                DrawItem("Temporary Cache Path", Application.temporaryCachePath);
                DrawItem("Console Log Path", Application.consoleLogPath);
            }
            GUILayout.EndVertical();
        }
    }
}