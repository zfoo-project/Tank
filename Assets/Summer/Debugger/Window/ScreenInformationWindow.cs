using Summer.Debugger.Window.Model;
using Spring.Util;
using UnityEngine;

namespace Summer.Debugger.Window
{
    public sealed class ScreenInformationWindow : ScrollableDebuggerWindowBase
    {
        protected override void OnDrawScrollableWindow()
        {
            GUILayout.Label("<b>Screen Information</b>");
            GUILayout.BeginVertical("box");
            {
                DrawItem("Current Resolution", GetResolutionString(Screen.currentResolution));
                DrawItem("Screen Width",
                    StringUtils.Format("{} px / {} in / {} cm", Screen.width.ToString(), ConverterUtils.GetInchesFromPixels(Screen.width).ToString("F2"), ConverterUtils.GetCentimetersFromPixels(Screen.width).ToString("F2")));
                DrawItem("Screen Height",
                    StringUtils.Format("{} px / {} in / {} cm", Screen.height.ToString(), ConverterUtils.GetInchesFromPixels(Screen.height).ToString("F2"), ConverterUtils.GetCentimetersFromPixels(Screen.height).ToString("F2")));
                DrawItem("Screen DPI", Screen.dpi.ToString("F2"));
                DrawItem("Screen Orientation", Screen.orientation.ToString());
                DrawItem("Is Full Screen", Screen.fullScreen.ToString());
                DrawItem("Full Screen Mode", Screen.fullScreenMode.ToString());
                DrawItem("Sleep Timeout", GetSleepTimeoutDescription(Screen.sleepTimeout));
                DrawItem("Brightness", Screen.brightness.ToString("F2"));
                DrawItem("Cursor Visible", Cursor.visible.ToString());
                DrawItem("Cursor Lock State", Cursor.lockState.ToString());
                DrawItem("Auto Landscape Left", Screen.autorotateToLandscapeLeft.ToString());
                DrawItem("Auto Landscape Right", Screen.autorotateToLandscapeRight.ToString());
                DrawItem("Auto Portrait", Screen.autorotateToPortrait.ToString());
                DrawItem("Auto Portrait Upside Down", Screen.autorotateToPortraitUpsideDown.ToString());
                DrawItem("Safe Area", Screen.safeArea.ToString());
                DrawItem("Cutouts", GetCutoutsString(Screen.cutouts));
                DrawItem("Support Resolutions", GetResolutionsString(Screen.resolutions));
            }
            GUILayout.EndVertical();
        }

        private string GetSleepTimeoutDescription(int sleepTimeout)
        {
            if (sleepTimeout == SleepTimeout.NeverSleep)
            {
                return "Never Sleep";
            }

            if (sleepTimeout == SleepTimeout.SystemSetting)
            {
                return "System Setting";
            }

            return sleepTimeout.ToString();
        }

        private string GetResolutionString(Resolution resolution)
        {
            return StringUtils.Format("{} x {} @ {}Hz", resolution.width.ToString(), resolution.height.ToString(), resolution.refreshRate.ToString());
        }

        private string GetCutoutsString(Rect[] cutouts)
        {
            string[] cutoutStrings = new string[cutouts.Length];
            for (int i = 0; i < cutouts.Length; i++)
            {
                cutoutStrings[i] = cutouts[i].ToString();
            }

            return string.Join("; ", cutoutStrings);
        }

        private string GetResolutionsString(Resolution[] resolutions)
        {
            string[] resolutionStrings = new string[resolutions.Length];
            for (int i = 0; i < resolutions.Length; i++)
            {
                resolutionStrings[i] = GetResolutionString(resolutions[i]);
            }

            return string.Join("; ", resolutionStrings);
        }
    }
}