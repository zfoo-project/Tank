using System.Collections.Generic;
using Summer.ObjectPool;
using Spring.Core;
using Spring.Util;
using Summer.Debugger.Manager;
using Summer.Debugger.Window;
using Summer.Debugger.Window.Model;
using Summer.Debugger.Window.Model.VO;
using UnityEngine;
using SpringComponent = Summer.Base.SpringComponent;

namespace Summer.Debugger
{
    /// <summary>
    /// 调试器组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Summer/Debugger")]
    public sealed class DebuggerComponent : SpringComponent
    {
        /// <summary>
        /// 默认调试器漂浮框大小。
        /// </summary>
        public static readonly Rect DefaultIconRect = new Rect(10f, 10f, 60f, 60f);

        /// <summary>
        /// 默认调试器窗口大小。
        /// </summary>
        public static readonly Rect DefaultWindowRect = new Rect(10f, 10f, 640f, 480f);

        /// <summary>
        /// 默认调试器窗口缩放比例。
        /// </summary>
        public static readonly float DefaultWindowScale = 1f;

        [Autowired]
        private IDebuggerManager debuggerManager;

        private Rect dragRect = new Rect(0f, 0f, float.MaxValue, 25f);
        // 获取或设置调试器漂浮框大小。
        private Rect iconRect = DefaultIconRect;
        // 获取或设置调试器窗口大小。
        private Rect windowRect = DefaultWindowRect;
        // 获取或设置调试器窗口缩放比例。
        private float windowScale = DefaultWindowScale;

        [SerializeField]
        private GUISkin guiSkin;

        [SerializeField]
        private DebuggerActiveWindowType activeWindow = DebuggerActiveWindowType.AlwaysOpen;

        [SerializeField]
        private bool showFullWindow;

        [SerializeField]
        private ConsoleWindow consoleWindow = new ConsoleWindow();

        private SystemInformationWindow systemInformationWindow = new SystemInformationWindow();
        private EnvironmentInformationWindow environmentInformationWindow = new EnvironmentInformationWindow();
        private ScreenInformationWindow screenInformationWindow = new ScreenInformationWindow();
        private GraphicsInformationWindow graphicsInformationWindow = new GraphicsInformationWindow();
        private InputSummaryInformationWindow inputSummaryInformationWindow = new InputSummaryInformationWindow();
        private InputTouchInformationWindow inputTouchInformationWindow = new InputTouchInformationWindow();
        private InputLocationInformationWindow inputLocationInformationWindow = new InputLocationInformationWindow();

        private InputAccelerationInformationWindow inputAccelerationInformationWindow = new InputAccelerationInformationWindow();

        private InputGyroscopeInformationWindow inputGyroscopeInformationWindow = new InputGyroscopeInformationWindow();

        private InputCompassInformationWindow inputCompassInformationWindow = new InputCompassInformationWindow();
        private PathInformationWindow pathInformationWindow = new PathInformationWindow();
        private SceneInformationWindow sceneInformationWindow = new SceneInformationWindow();
        private TimeInformationWindow timeInformationWindow = new TimeInformationWindow();
        private QualityInformationWindow qualityInformationWindow = new QualityInformationWindow();
        private ProfilerInformationWindow profilerInformationWindow = new ProfilerInformationWindow();
        private WebPlayerInformationWindow webPlayerInformationWindow = new WebPlayerInformationWindow();
        private RuntimeMemorySummaryWindow runtimeMemorySummaryWindow = new RuntimeMemorySummaryWindow();

        private RuntimeMemoryInformationWindow<Object> runtimeMemoryAllInformationWindow = new RuntimeMemoryInformationWindow<Object>();

        private RuntimeMemoryInformationWindow<Texture> runtimeMemoryTextureInformationWindow = new RuntimeMemoryInformationWindow<Texture>();

        private RuntimeMemoryInformationWindow<Mesh> runtimeMemoryMeshInformationWindow = new RuntimeMemoryInformationWindow<Mesh>();

        private RuntimeMemoryInformationWindow<Material> runtimeMemoryMaterialInformationWindow = new RuntimeMemoryInformationWindow<Material>();

        private RuntimeMemoryInformationWindow<Shader> runtimeMemoryShaderInformationWindow = new RuntimeMemoryInformationWindow<Shader>();

        private RuntimeMemoryInformationWindow<AnimationClip> runtimeMemoryAnimationClipInformationWindow = new RuntimeMemoryInformationWindow<AnimationClip>();

        private RuntimeMemoryInformationWindow<AudioClip> runtimeMemoryAudioClipInformationWindow = new RuntimeMemoryInformationWindow<AudioClip>();

        private RuntimeMemoryInformationWindow<Font> runtimeMemoryFontInformationWindow = new RuntimeMemoryInformationWindow<Font>();

        private RuntimeMemoryInformationWindow<TextAsset> runtimeMemoryTextAssetInformationWindow = new RuntimeMemoryInformationWindow<TextAsset>();

        private RuntimeMemoryInformationWindow<ScriptableObject> runtimeMemoryScriptableObjectInformationWindow = new RuntimeMemoryInformationWindow<ScriptableObject>();

        private ObjectPoolInformationWindow objectPoolInformationWindow = new ObjectPoolInformationWindow();
        private ReferencePoolInformationWindow referencePoolInformationWindow = new ReferencePoolInformationWindow();
        private OperationsWindow operationsWindow = new OperationsWindow();

        private FpsCounter fpsCounter;


        [BeforePostConstruct]
        private void Init()
        {
            switch (activeWindow)
            {
                case DebuggerActiveWindowType.AlwaysOpen:
                    debuggerManager.ActiveWindow = true;
                    break;

                case DebuggerActiveWindowType.OnlyOpenWhenDevelopment:
                    debuggerManager.ActiveWindow = Debug.isDebugBuild;
                    break;

                case DebuggerActiveWindowType.OnlyOpenInEditor:
                    debuggerManager.ActiveWindow = Application.isEditor;
                    break;

                default:
                    debuggerManager.ActiveWindow = false;
                    break;
            }

            fpsCounter = new FpsCounter(0.5f);

            RegisterDebuggerWindow("Console", consoleWindow);
            RegisterDebuggerWindow("Information/System", systemInformationWindow);
            RegisterDebuggerWindow("Information/Environment", environmentInformationWindow);
            RegisterDebuggerWindow("Information/Screen", screenInformationWindow);
            RegisterDebuggerWindow("Information/Graphics", graphicsInformationWindow);
            RegisterDebuggerWindow("Information/Input/Summary", inputSummaryInformationWindow);
            RegisterDebuggerWindow("Information/Input/Touch", inputTouchInformationWindow);
            RegisterDebuggerWindow("Information/Input/Location", inputLocationInformationWindow);
            RegisterDebuggerWindow("Information/Input/Acceleration", inputAccelerationInformationWindow);
            RegisterDebuggerWindow("Information/Input/Gyroscope", inputGyroscopeInformationWindow);
            RegisterDebuggerWindow("Information/Input/Compass", inputCompassInformationWindow);
            RegisterDebuggerWindow("Information/Other/Scene", sceneInformationWindow);
            RegisterDebuggerWindow("Information/Other/Path", pathInformationWindow);
            RegisterDebuggerWindow("Information/Other/Time", timeInformationWindow);
            RegisterDebuggerWindow("Information/Other/Quality", qualityInformationWindow);
            RegisterDebuggerWindow("Information/Other/Web Player", webPlayerInformationWindow);
            RegisterDebuggerWindow("Profiler/Summary", profilerInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/Summary", runtimeMemorySummaryWindow);
            RegisterDebuggerWindow("Profiler/Memory/All", runtimeMemoryAllInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/Texture", runtimeMemoryTextureInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/Mesh", runtimeMemoryMeshInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/Material", runtimeMemoryMaterialInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/Shader", runtimeMemoryShaderInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/AnimationClip", runtimeMemoryAnimationClipInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/AudioClip", runtimeMemoryAudioClipInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/Font", runtimeMemoryFontInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/TextAsset", runtimeMemoryTextAssetInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/ScriptableObject",
                runtimeMemoryScriptableObjectInformationWindow);
            if (SpringContext.GetBean<ObjectPoolComponent>() != null)
            {
                RegisterDebuggerWindow("Profiler/Object Pool", objectPoolInformationWindow);
            }

            RegisterDebuggerWindow("Profiler/Reference Pool", referencePoolInformationWindow);

            RegisterDebuggerWindow("Other/Operations", operationsWindow);
        }

        private void Update()
        {
            fpsCounter.Update(Time.deltaTime, Time.unscaledDeltaTime);
        }

        private void OnGUI()
        {
            if (debuggerManager == null || !debuggerManager.ActiveWindow)
            {
                return;
            }

            GUISkin cachedGuiSkin = GUI.skin;
            Matrix4x4 cachedMatrix = GUI.matrix;

            GUI.skin = guiSkin;
            GUI.matrix = Matrix4x4.Scale(new Vector3(windowScale, windowScale, 1f));

            if (showFullWindow)
            {
                windowRect = GUILayout.Window(0, windowRect, DrawWindow, "<b>GAME FRAMEWORK DEBUGGER</b>");
            }
            else
            {
                iconRect = GUILayout.Window(0, iconRect, DrawDebuggerWindowIcon, "<b>DEBUGGER</b>");
            }

            GUI.matrix = cachedMatrix;
            GUI.skin = cachedGuiSkin;
        }

        /// <summary>
        /// 注册调试器窗口。
        /// </summary>
        /// <param name="path">调试器窗口路径。</param>
        /// <param name="debuggerWindow">要注册的调试器窗口。</param>
        /// <param name="args">初始化调试器窗口参数。</param>
        public void RegisterDebuggerWindow(string path, IDebuggerWindow debuggerWindow, params object[] args)
        {
            debuggerManager.RegisterDebuggerWindow(path, debuggerWindow, args);
        }

        /// <summary>
        /// 解除注册调试器窗口。
        /// </summary>
        /// <param name="path">调试器窗口路径。</param>
        /// <returns>是否解除注册调试器窗口成功。</returns>
        public bool UnregisterDebuggerWindow(string path)
        {
            return debuggerManager.UnregisterDebuggerWindow(path);
        }

        /// <summary>
        /// 获取调试器窗口。
        /// </summary>
        /// <param name="path">调试器窗口路径。</param>
        /// <returns>要获取的调试器窗口。</returns>
        public IDebuggerWindow GetDebuggerWindow(string path)
        {
            return debuggerManager.GetDebuggerWindow(path);
        }

        /// <summary>
        /// 选中调试器窗口。
        /// </summary>
        /// <param name="path">调试器窗口路径。</param>
        /// <returns>是否成功选中调试器窗口。</returns>
        public bool SelectDebuggerWindow(string path)
        {
            return debuggerManager.SelectDebuggerWindow(path);
        }

        /// <summary>
        /// 还原调试器窗口布局。
        /// </summary>
        public void ResetLayout()
        {
            iconRect = DefaultIconRect;
            windowRect = DefaultWindowRect;
            windowScale = DefaultWindowScale;
        }

        /// <summary>
        /// 获取记录的全部日志。
        /// </summary>
        /// <param name="results">要获取的日志。</param>
        public void GetRecentLogs(List<LogNode> results)
        {
            consoleWindow.GetRecentLogs(results);
        }

        /// <summary>
        /// 获取记录的最近日志。
        /// </summary>
        /// <param name="results">要获取的日志。</param>
        /// <param name="count">要获取最近日志的数量。</param>
        public void GetRecentLogs(List<LogNode> results, int count)
        {
            consoleWindow.GetRecentLogs(results, count);
        }

        private void DrawWindow(int windowId)
        {
            GUI.DragWindow(dragRect);
            DrawDebuggerWindowGroup(debuggerManager.DebuggerWindowRoot);
        }

        private void DrawDebuggerWindowGroup(IDebuggerWindowGroup debuggerWindowGroup)
        {
            if (debuggerWindowGroup == null)
            {
                return;
            }

            List<string> names = new List<string>();
            string[] debuggerWindowNames = debuggerWindowGroup.GetDebuggerWindowNames();
            for (int i = 0; i < debuggerWindowNames.Length; i++)
            {
                names.Add(StringUtils.Format("<b>{}</b>", debuggerWindowNames[i]));
            }

            if (debuggerWindowGroup == debuggerManager.DebuggerWindowRoot)
            {
                names.Add("<b>Close</b>");
            }

            int toolbarIndex = GUILayout.Toolbar(debuggerWindowGroup.SelectedIndex, names.ToArray(),
                GUILayout.Height(30f), GUILayout.MaxWidth(Screen.width));
            if (toolbarIndex >= debuggerWindowGroup.DebuggerWindowCount)
            {
                showFullWindow = false;
                return;
            }

            if (debuggerWindowGroup.SelectedWindow == null)
            {
                return;
            }

            if (debuggerWindowGroup.SelectedIndex != toolbarIndex)
            {
                debuggerWindowGroup.SelectedWindow.OnLeave();
                debuggerWindowGroup.SelectedIndex = toolbarIndex;
                debuggerWindowGroup.SelectedWindow.OnEnter();
            }

            IDebuggerWindowGroup subDebuggerWindowGroup = debuggerWindowGroup.SelectedWindow as IDebuggerWindowGroup;
            if (subDebuggerWindowGroup != null)
            {
                DrawDebuggerWindowGroup(subDebuggerWindowGroup);
            }

            debuggerWindowGroup.SelectedWindow.OnDraw();
        }

        private void DrawDebuggerWindowIcon(int windowId)
        {
            GUI.DragWindow(dragRect);
            GUILayout.Space(5);
            Color32 color = Color.white;
            consoleWindow.RefreshCount();
            if (consoleWindow.FatalCount > 0)
            {
                color = consoleWindow.GetLogStringColor(LogType.Exception);
            }
            else if (consoleWindow.ErrorCount > 0)
            {
                color = consoleWindow.GetLogStringColor(LogType.Error);
            }
            else if (consoleWindow.WarningCount > 0)
            {
                color = consoleWindow.GetLogStringColor(LogType.Warning);
            }
            else
            {
                color = consoleWindow.GetLogStringColor(LogType.Log);
            }

            string title = StringUtils.Format("<color=#{}{}{}{}><b>FPS: {}</b></color>", color.r.ToString("x2"),
                color.g.ToString("x2"), color.b.ToString("x2"), color.a.ToString("x2"),
                fpsCounter.CurrentFps.ToString("F2"));
            if (GUILayout.Button(title, GUILayout.Width(100f), GUILayout.Height(40f)))
            {
                showFullWindow = true;
            }
        }
    }
}