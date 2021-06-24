using Summer.Base.Model;
using Spring.Util;
using Summer.Editor.ResourceBuilder.Model;
using UnityEditor;
using UnityEngine;

namespace Summer.Editor.ResourceBuilder
{
    /// <summary>
    /// 生成资源。
    /// </summary>
    public static class BuildResources
    {
        /// <summary>
        /// 运行生成资源。
        /// </summary>
        [MenuItem("Summer/Resource Tools/Build Resources", false, 40)]
        public static void Run()
        {
            Run(null, Platform.Undefined, null, null);
        }

        public static void Run(int internalResourceVersion)
        {
            Run((int?)internalResourceVersion, Platform.Undefined, null, null);
        }

        public static void Run(Platform platforms)
        {
            Run(null, platforms, null, null);
        }

        public static void Run(string outputDirectory)
        {
            Run(null, Platform.Undefined, outputDirectory, null);
        }

        public static void Run(int internalResourceVersion, Platform platforms)
        {
            Run((int?)internalResourceVersion, platforms, null, null);
        }

        public static void Run(int internalResourceVersion, string outputDirectory)
        {
            Run((int?)internalResourceVersion, Platform.Undefined, outputDirectory, null);
        }

        public static void Run(Platform platforms, string outputDirectory)
        {
            Run(null, platforms, outputDirectory, null);
        }

        public static void Run(string outputDirectory, string buildEventHandlerTypeName)
        {
            Run(null, Platform.Undefined, outputDirectory, buildEventHandlerTypeName);
        }

        public static void Run(int internalResourceVersion, Platform platforms, string outputDirectory)
        {
            Run((int?)internalResourceVersion, platforms, outputDirectory, null);
        }

        public static void Run(int internalResourceVersion, string outputDirectory, string buildEventHandlerTypeName)
        {
            Run((int?)internalResourceVersion, Platform.Undefined, outputDirectory, buildEventHandlerTypeName);
        }

        public static void Run(Platform platforms, string outputDirectory, string buildEventHandlerTypeName)
        {
            Run(null, platforms, outputDirectory, buildEventHandlerTypeName);
        }

        public static void Run(int internalResourceVersion, Platform platforms, string outputDirectory, string buildEventHandlerTypeName)
        {
            Run((int?)internalResourceVersion, platforms, outputDirectory, buildEventHandlerTypeName);
        }

        private static void Run(int? internalResourceVersion, Platform platforms, string outputDirectory, string buildEventHandlerTypeName)
        {
            ResourceBuilderController controller = new ResourceBuilderController();
            if (!controller.Load())
            {
                throw new GameFrameworkException("Load configuration failure.");
            }
            else
            {
                Debug.Log("Load configuration success.");
            }

            if (platforms != Platform.Undefined)
            {
                controller.Platforms = platforms;
            }

            if (internalResourceVersion.HasValue)
            {
                controller.InternalResourceVersion = internalResourceVersion.Value;
            }

            if (outputDirectory != null)
            {
                controller.OutputDirectory = outputDirectory;
            }

            if (buildEventHandlerTypeName != null)
            {
                controller.BuildEventHandlerTypeName = buildEventHandlerTypeName;
            }

            if (!controller.IsValidOutputDirectory)
            {
                throw new GameFrameworkException(StringUtils.Format("Output directory '{}' is invalid.", controller.OutputDirectory));
            }

            if (!controller.BuildResources())
            {
                throw new GameFrameworkException("Build resources failure.");
            }
            else
            {
                Debug.Log("Build resources success.");
                controller.Save();
            }
        }
    }
}
