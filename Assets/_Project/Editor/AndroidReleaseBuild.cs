using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ColorFactory.EditorTools
{
    /// <summary>
    /// Build APK Android cho Development và Release candidate, từ menu hoặc command line (-executeMethod).
    /// Chỉ đọc scene trong Build Profile và xuất file vào thư mục Builds/ ngoài Assets (đã được .gitignore).
    /// </summary>
    public static class AndroidReleaseBuild
    {
        /// <summary>
        /// Menu: build APK Development (có Autoconnect Profiler và Script Debugging) cho máy thử.
        /// </summary>
        [MenuItem("Color Factory/Build/Android APK (Development)")]
        public static void BuildDevelopmentApk()
        {
            Build(development: true);
        }

        /// <summary>
        /// Menu: build APK Release candidate, không có debug và không có profiler.
        /// </summary>
        [MenuItem("Color Factory/Build/Android APK (Release)")]
        public static void BuildReleaseApk()
        {
            Build(development: false);
        }

        /// <summary>
        /// Entry point cho command line: build APK Development rồi thoát với mã lỗi phù hợp.
        /// </summary>
        public static void BuildDevelopmentFromCommandLine()
        {
            Build(development: true);
        }

        /// <summary>
        /// Entry point cho command line: build APK Release rồi thoát với mã lỗi phù hợp.
        /// </summary>
        public static void BuildReleaseFromCommandLine()
        {
            Build(development: false);
        }

        /// <summary>
        /// Chuyển sang Android, đọc scene đã bật trong Build Profile, build APK và ghi kết quả.
        /// Trong batch mode, thoát Editor với mã 1 nếu build lỗi hoặc không có scene nào.
        /// </summary>
        private static void Build(bool development)
        {
            string[] scenes = GetEnabledScenes();
            if (scenes.Length == 0)
            {
                Fail("Build Profile không có scene nào được bật.");
                return;
            }

            EditorUserBuildSettings.SwitchActiveBuildTarget(
                BuildTargetGroup.Android,
                BuildTarget.Android);

            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.development = development;
            EditorUserBuildSettings.allowDebugging = development;
            EditorUserBuildSettings.connectProfiler = development;

            string outputPath = GetOutputPath(development);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = development
                    ? BuildOptions.Development | BuildOptions.AllowDebugging
                    : BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result != BuildResult.Succeeded)
            {
                Fail($"Build APK thất bại: {summary.result}, lỗi: {summary.totalErrors}.");
                return;
            }

            Debug.Log(
                $"[AndroidReleaseBuild] PASS: {outputPath} " +
                $"({summary.totalSize / (1024 * 1024)} MB, {scenes.Length} scene, " +
                $"{(development ? "Development" : "Release")}).");

            ExitIfBatch(0);
        }

        /// <summary>
        /// Lấy đường dẫn các scene đang bật trong Build Profile, theo thứ tự đã cấu hình.
        /// </summary>
        private static string[] GetEnabledScenes()
        {
            var paths = new System.Collections.Generic.List<string>();

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                    paths.Add(scene.path);
            }

            return paths.ToArray();
        }

        /// <summary>
        /// Tạo đường dẫn APK trong Builds/Android, tên gồm version và loại build.
        /// </summary>
        private static string GetOutputPath(bool development)
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string kind = development ? "dev" : "release";
            string fileName = $"ColorFactory-{PlayerSettings.bundleVersion}-{kind}.apk";

            return Path.Combine(projectRoot, "Builds", "Android", fileName);
        }

        /// <summary>
        /// Ghi lỗi ra Console và thoát Editor với mã 1 nếu đang chạy batch mode.
        /// </summary>
        private static void Fail(string message)
        {
            Debug.LogError($"[AndroidReleaseBuild] FAIL: {message}");
            ExitIfBatch(1);
        }

        /// <summary>
        /// Thoát Editor với mã cho trước khi chạy batch mode; không làm gì trong Editor thường.
        /// </summary>
        private static void ExitIfBatch(int exitCode)
        {
            if (Application.isBatchMode)
                EditorApplication.Exit(exitCode);
        }
    }
}
