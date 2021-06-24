using Summer.Debugger.Window.Model;
using UnityEngine;

namespace Summer.Debugger.Window
{
    public sealed class WebPlayerInformationWindow : ScrollableDebuggerWindowBase
    {
        protected override void OnDrawScrollableWindow()
        {
            GUILayout.Label("<b>Web Player Information</b>");
            GUILayout.BeginVertical("box");
            {
                DrawItem("Absolute URL", Application.absoluteURL);
            }
            GUILayout.EndVertical();
        }
    }
}