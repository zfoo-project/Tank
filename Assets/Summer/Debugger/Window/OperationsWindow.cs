using Summer.Base;
using Summer.Debugger.Window.Model;
using Summer.ObjectPool;
using Summer.Resource;
using Spring.Core;
using UnityEngine;

namespace Summer.Debugger.Window
{
    public sealed class OperationsWindow : ScrollableDebuggerWindowBase
    {
        protected override void OnDrawScrollableWindow()
        {
            GUILayout.Label("<b>Operations</b>");
            GUILayout.BeginVertical("box");
            {
                var objectPoolManager = SpringContext.GetBean<IObjectPoolManager>();
                if (GUILayout.Button("Object Pool Release", GUILayout.Height(30f)))
                {
                    objectPoolManager.Release();
                }

                if (GUILayout.Button("Object Pool Release All Unused", GUILayout.Height(30f)))
                {
                    objectPoolManager.ReleaseAllUnused();
                }

                ResourceComponent resourceCompoent = SpringContext.GetBean<ResourceComponent>();
                if (resourceCompoent != null)
                {
                    if (GUILayout.Button("Unload Unused Assets", GUILayout.Height(30f)))
                    {
                        resourceCompoent.ForceUnloadUnusedAssets(false);
                    }

                    if (GUILayout.Button("Unload Unused Assets and Garbage Collect", GUILayout.Height(30f)))
                    {
                        resourceCompoent.ForceUnloadUnusedAssets(true);
                    }
                }

                if (GUILayout.Button("Shutdown Game Framework (None)", GUILayout.Height(30f)))
                {
                    SpringContext.GetBean<BaseComponent>().Shutdown(ShutdownType.None);
                }

                if (GUILayout.Button("Shutdown Game Framework (Restart)", GUILayout.Height(30f)))
                {
                    SpringContext.GetBean<BaseComponent>().Shutdown(ShutdownType.Restart);
                }

                if (GUILayout.Button("Shutdown Game Framework (Quit)", GUILayout.Height(30f)))
                {
                    SpringContext.GetBean<BaseComponent>().Shutdown(ShutdownType.Quit);
                }
            }
            GUILayout.EndVertical();
        }
    }
}