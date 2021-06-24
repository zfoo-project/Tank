using Summer.Debugger.Window.Model;
using Spring.Util;
using UnityEngine;

namespace Summer.Debugger.Window
{
    public sealed class TimeInformationWindow : ScrollableDebuggerWindowBase
    {
        protected override void OnDrawScrollableWindow()
        {
            GUILayout.Label("<b>Time Information</b>");
            GUILayout.BeginVertical("box");
            {
                DrawItem("Time Scale", StringUtils.Format("{} [{}]", Time.timeScale.ToString(), GetTimeScaleDescription(Time.timeScale)));
                DrawItem("Realtime Since Startup", Time.realtimeSinceStartup.ToString());
                DrawItem("Time Since Level Load", Time.timeSinceLevelLoad.ToString());
                DrawItem("Time", Time.time.ToString());
                DrawItem("Fixed Time", Time.fixedTime.ToString());
                DrawItem("Unscaled Time", Time.unscaledTime.ToString());
                DrawItem("Fixed Unscaled Time", Time.fixedUnscaledTime.ToString());
                DrawItem("Delta Time", Time.deltaTime.ToString());
                DrawItem("Fixed Delta Time", Time.fixedDeltaTime.ToString());
                DrawItem("Unscaled Delta Time", Time.unscaledDeltaTime.ToString());
                DrawItem("Fixed Unscaled Delta Time", Time.fixedUnscaledDeltaTime.ToString());
                DrawItem("Smooth Delta Time", Time.smoothDeltaTime.ToString());
                DrawItem("Maximum Delta Time", Time.maximumDeltaTime.ToString());
                DrawItem("Maximum Particle Delta Time", Time.maximumParticleDeltaTime.ToString());
                DrawItem("Frame Count", Time.frameCount.ToString());
                DrawItem("Rendered Frame Count", Time.renderedFrameCount.ToString());
                DrawItem("Capture Framerate", Time.captureFramerate.ToString());
                DrawItem("Capture Delta Time", Time.captureDeltaTime.ToString());
                DrawItem("In Fixed Time Step", Time.inFixedTimeStep.ToString());
            }
            GUILayout.EndVertical();
        }

        private string GetTimeScaleDescription(float timeScale)
        {
            if (timeScale <= 0f)
            {
                return "Pause";
            }

            if (timeScale < 1f)
            {
                return "Slower";
            }

            if (timeScale > 1f)
            {
                return "Faster";
            }

            return "Normal";
        }
    }
}