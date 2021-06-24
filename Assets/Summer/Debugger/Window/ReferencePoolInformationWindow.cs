using System;
using System.Collections.Generic;
using Summer.Debugger.Window.Model;
using Spring.Collection.Reference;
using Spring.Util;
using UnityEngine;

namespace Summer.Debugger.Window
{
    public sealed class ReferencePoolInformationWindow : ScrollableDebuggerWindowBase
    {
        private readonly Dictionary<string, List<ReferenceCacheInfo>> referencePoolInfos = new Dictionary<string, List<ReferenceCacheInfo>>(StringComparer.Ordinal);
        private bool showFullClassName = false;

        public override void Initialize(params object[] args)
        {
        }

        protected override void OnDrawScrollableWindow()
        {
            GUILayout.Label("<b>Reference Pool Information</b>");
            GUILayout.BeginVertical("box");
            {
                DrawItem("Reference Pool Count", ReferenceCache.Count.ToString());
            }
            GUILayout.EndVertical();

            showFullClassName = GUILayout.Toggle(showFullClassName, "Show Full Class Name");
            this.referencePoolInfos.Clear();
            ReferenceCacheInfo[] referencePoolInfos = ReferenceCache.GetAllReferencePoolInfos();
            foreach (ReferenceCacheInfo referencePoolInfo in referencePoolInfos)
            {
                string assemblyName = referencePoolInfo.Type.Assembly.GetName().Name;
                List<ReferenceCacheInfo> results = null;
                if (!this.referencePoolInfos.TryGetValue(assemblyName, out results))
                {
                    results = new List<ReferenceCacheInfo>();
                    this.referencePoolInfos.Add(assemblyName, results);
                }

                results.Add(referencePoolInfo);
            }

            foreach (KeyValuePair<string, List<ReferenceCacheInfo>> assemblyReferencePoolInfo in this.referencePoolInfos)
            {
                GUILayout.Label(StringUtils.Format("<b>Assembly: {}</b>", assemblyReferencePoolInfo.Key));
                GUILayout.BeginVertical("box");
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label(showFullClassName ? "<b>Full Class Name</b>" : "<b>Class Name</b>");
                        GUILayout.Label("<b>Unused</b>", GUILayout.Width(60f));
                        GUILayout.Label("<b>Using</b>", GUILayout.Width(60f));
                        GUILayout.Label("<b>Acquire</b>", GUILayout.Width(60f));
                        GUILayout.Label("<b>Release</b>", GUILayout.Width(60f));
                        GUILayout.Label("<b>Add</b>", GUILayout.Width(60f));
                        GUILayout.Label("<b>Remove</b>", GUILayout.Width(60f));
                    }
                    GUILayout.EndHorizontal();

                    if (assemblyReferencePoolInfo.Value.Count > 0)
                    {
                        assemblyReferencePoolInfo.Value.Sort(Comparison);
                        foreach (ReferenceCacheInfo referencePoolInfo in assemblyReferencePoolInfo.Value)
                        {
                            DrawReferencePoolInfo(referencePoolInfo);
                        }
                    }
                    else
                    {
                        GUILayout.Label("<i>Reference Pool is Empty ...</i>");
                    }
                }
                GUILayout.EndVertical();
            }
        }

        private void DrawReferencePoolInfo(ReferenceCacheInfo referenceCacheInfo)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(showFullClassName ? referenceCacheInfo.Type.FullName : referenceCacheInfo.Type.Name);
                GUILayout.Label(referenceCacheInfo.UnusedReferenceCount.ToString(), GUILayout.Width(60f));
                GUILayout.Label(referenceCacheInfo.UsingReferenceCount.ToString(), GUILayout.Width(60f));
                GUILayout.Label(referenceCacheInfo.AcquireReferenceCount.ToString(), GUILayout.Width(60f));
                GUILayout.Label(referenceCacheInfo.ReleaseReferenceCount.ToString(), GUILayout.Width(60f));
                GUILayout.Label(referenceCacheInfo.AddReferenceCount.ToString(), GUILayout.Width(60f));
                GUILayout.Label(referenceCacheInfo.RemoveReferenceCount.ToString(), GUILayout.Width(60f));
            }
            GUILayout.EndHorizontal();
        }

        private int Comparison(ReferenceCacheInfo a, ReferenceCacheInfo b)
        {
            if (showFullClassName)
            {
                return a.Type.FullName.CompareTo(b.Type.FullName);
            }
            else
            {
                return a.Type.Name.CompareTo(b.Type.Name);
            }
        }
    }
}