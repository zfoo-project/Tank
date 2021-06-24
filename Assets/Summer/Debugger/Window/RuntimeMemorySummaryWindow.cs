using System;
using System.Collections.Generic;
using Summer.Debugger.Window.Model;
using Summer.Debugger.Window.Model.VO;
using Spring.Util;
using UnityEngine;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;

namespace Summer.Debugger.Window
{
    public sealed class RuntimeMemorySummaryWindow : ScrollableDebuggerWindowBase
    {
        private readonly List<Record> records = new List<Record>();
        private DateTime sampleTime = DateTime.MinValue;
        private int sampleCount = 0;
        private long sampleSize = 0L;

        protected override void OnDrawScrollableWindow()
        {
            GUILayout.Label("<b>Runtime Memory Summary</b>");
            GUILayout.BeginVertical("box");
            {
                if (GUILayout.Button("Take Sample", GUILayout.Height(30f)))
                {
                    TakeSample();
                }

                if (sampleTime <= DateTime.MinValue)
                {
                    GUILayout.Label("<b>Please take sample first.</b>");
                }
                else
                {
                    GUILayout.Label(StringUtils.Format("<b>{} Objects ({}) obtained at {}.</b>", sampleCount.ToString(), GetByteLengthString(sampleSize), sampleTime.ToString("yyyy-MM-dd HH:mm:ss")));

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("<b>Type</b>");
                        GUILayout.Label("<b>Count</b>", GUILayout.Width(120f));
                        GUILayout.Label("<b>Size</b>", GUILayout.Width(120f));
                    }
                    GUILayout.EndHorizontal();

                    for (int i = 0; i < records.Count; i++)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label(records[i].Name);
                            GUILayout.Label(records[i].Count.ToString(), GUILayout.Width(120f));
                            GUILayout.Label(GetByteLengthString(records[i].Size), GUILayout.Width(120f));
                        }
                        GUILayout.EndHorizontal();
                    }
                }
            }
            GUILayout.EndVertical();
        }

        private void TakeSample()
        {
            records.Clear();
            sampleTime = DateTime.UtcNow;
            sampleCount = 0;
            sampleSize = 0L;

            Object[] samples = Resources.FindObjectsOfTypeAll<Object>();
            for (int i = 0; i < samples.Length; i++)
            {
                long sampleSize = 0L;
                sampleSize = Profiler.GetRuntimeMemorySizeLong(samples[i]);
                string name = samples[i].GetType().Name;
                sampleCount++;
                this.sampleSize += sampleSize;

                Record record = null;
                foreach (Record r in records)
                {
                    if (r.Name == name)
                    {
                        record = r;
                        break;
                    }
                }

                if (record == null)
                {
                    record = new Record(name);
                    records.Add(record);
                }

                record.Count++;
                record.Size += sampleSize;
            }

            records.Sort(RecordComparer);
        }

        private int RecordComparer(Record a, Record b)
        {
            int result = b.Size.CompareTo(a.Size);
            if (result != 0)
            {
                return result;
            }

            result = a.Count.CompareTo(b.Count);
            if (result != 0)
            {
                return result;
            }

            return a.Name.CompareTo(b.Name);
        }
    }
}