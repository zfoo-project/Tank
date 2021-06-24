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
    public sealed partial class RuntimeMemoryInformationWindow<T> : ScrollableDebuggerWindowBase where T : Object
    {
        private const int ShowSampleCount = 300;

        private readonly List<Sample> samples = new List<Sample>();
        private DateTime sampleTime = DateTime.MinValue;
        private long sampleSize = 0L;
        private long duplicateSampleSize = 0L;
        private int duplicateSimpleCount = 0;

        protected override void OnDrawScrollableWindow()
        {
            string typeName = typeof(T).Name;
            GUILayout.Label(StringUtils.Format("<b>{} Runtime Memory Information</b>", typeName));
            GUILayout.BeginVertical("box");
            {
                if (GUILayout.Button(StringUtils.Format("Take Sample for {}", typeName), GUILayout.Height(30f)))
                {
                    TakeSample();
                }

                if (sampleTime <= DateTime.MinValue)
                {
                    GUILayout.Label(StringUtils.Format("<b>Please take sample for {} first.</b>", typeName));
                }
                else
                {
                    if (duplicateSimpleCount > 0)
                    {
                        GUILayout.Label(StringUtils.Format("<b>{} {}s ({}) obtained at {}, while {} {}s ({}) might be duplicated.</b>", samples.Count.ToString(), typeName, GetByteLengthString(sampleSize),
                            sampleTime.ToString("yyyy-MM-dd HH:mm:ss"), duplicateSimpleCount.ToString(), GetByteLengthString(duplicateSampleSize)));
                    }
                    else
                    {
                        GUILayout.Label(StringUtils.Format("<b>{} {}s ({}) obtained at {}.</b>", samples.Count.ToString(), typeName, GetByteLengthString(sampleSize), sampleTime.ToString("yyyy-MM-dd HH:mm:ss")));
                    }

                    if (samples.Count > 0)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label(StringUtils.Format("<b>{} Name</b>", typeName));
                            GUILayout.Label("<b>Type</b>", GUILayout.Width(240f));
                            GUILayout.Label("<b>Size</b>", GUILayout.Width(80f));
                        }
                        GUILayout.EndHorizontal();
                    }

                    int count = 0;
                    for (int i = 0; i < samples.Count; i++)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label(samples[i].Highlight ? StringUtils.Format((string) "<color=yellow>{}</color>", (object) samples[i].Name) : samples[i].Name);
                            GUILayout.Label(samples[i].Highlight ? StringUtils.Format((string) "<color=yellow>{}</color>", (object) samples[i].Type) : samples[i].Type, GUILayout.Width(240f));
                            GUILayout.Label(samples[i].Highlight ? StringUtils.Format("<color=yellow>{}</color>", GetByteLengthString(samples[i].Size)) : GetByteLengthString(samples[i].Size), GUILayout.Width(80f));
                        }
                        GUILayout.EndHorizontal();

                        count++;
                        if (count >= ShowSampleCount)
                        {
                            break;
                        }
                    }
                }
            }
            GUILayout.EndVertical();
        }

        private void TakeSample()
        {
            sampleTime = DateTime.UtcNow;
            sampleSize = 0L;
            duplicateSampleSize = 0L;
            duplicateSimpleCount = 0;
            this.samples.Clear();

            T[] samples = Resources.FindObjectsOfTypeAll<T>();
            for (int i = 0; i < samples.Length; i++)
            {
                long sampleSize = 0L;
                sampleSize = Profiler.GetRuntimeMemorySizeLong(samples[i]);
                this.sampleSize += sampleSize;
                this.samples.Add(new Sample(samples[i].name, samples[i].GetType().Name, sampleSize));
            }

            this.samples.Sort(SampleComparer);

            for (int i = 1; i < this.samples.Count; i++)
            {
                if (this.samples[i].Name == this.samples[i - 1].Name && this.samples[i].Type == this.samples[i - 1].Type && this.samples[i].Size == this.samples[i - 1].Size)
                {
                    this.samples[i].Highlight = true;
                    duplicateSampleSize += this.samples[i].Size;
                    duplicateSimpleCount++;
                }
            }
        }

        private int SampleComparer(Sample a, Sample b)
        {
            int result = b.Size.CompareTo(a.Size);
            if (result != 0)
            {
                return result;
            }

            result = a.Type.CompareTo(b.Type);
            if (result != 0)
            {
                return result;
            }

            return a.Name.CompareTo(b.Name);
        }
    }
}