using Spring.Util;
using UnityEngine;

namespace Summer.Debugger.Window.Model
{
    public abstract class ScrollableDebuggerWindowBase : IDebuggerWindow
    {
        private const float TitleWidth = 240f;
        private Vector2 m_ScrollPosition = Vector2.zero;

        public virtual void Initialize(params object[] args)
        {
        }

        public virtual void Shutdown()
        {
        }

        public virtual void OnEnter()
        {
        }

        public virtual void OnLeave()
        {
        }

        public virtual void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
        }

        public void OnDraw()
        {
            m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition);
            {
                OnDrawScrollableWindow();
            }
            GUILayout.EndScrollView();
        }

        protected abstract void OnDrawScrollableWindow();

        protected static void DrawItem(string title, string content)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(title, GUILayout.Width(TitleWidth));
                GUILayout.Label(content);
            }
            GUILayout.EndHorizontal();
        }

        protected static string GetByteLengthString(long byteLength)
        {
            if (byteLength < 1024L) // 2 ^ 10
            {
                return StringUtils.Format("{} Bytes", byteLength.ToString());
            }

            if (byteLength < 1048576L) // 2 ^ 20
            {
                return StringUtils.Format("{} KB", (byteLength / 1024f).ToString("F2"));
            }

            if (byteLength < 1073741824L) // 2 ^ 30
            {
                return StringUtils.Format("{} MB", (byteLength / 1048576f).ToString("F2"));
            }

            if (byteLength < 1099511627776L) // 2 ^ 40
            {
                return StringUtils.Format("{} GB", (byteLength / 1073741824f).ToString("F2"));
            }

            if (byteLength < 1125899906842624L) // 2 ^ 50
            {
                return StringUtils.Format("{} TB", (byteLength / 1099511627776f).ToString("F2"));
            }

            if (byteLength < 1152921504606846976L) // 2 ^ 60
            {
                return StringUtils.Format("{} PB", (byteLength / 1125899906842624f).ToString("F2"));
            }

            return StringUtils.Format("{} EB", (byteLength / 1152921504606846976f).ToString("F2"));
        }
    }
}