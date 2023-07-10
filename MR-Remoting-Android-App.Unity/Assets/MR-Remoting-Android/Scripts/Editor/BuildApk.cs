using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;
using System.IO;

public class BuildApk
{
    static readonly string[] Scenes = {
        "Assets/MR-Remoting-Android/Scenes/main.unity"
    };

    [MenuItem("Tools/Build MR Remoting Android APK")]
    static void Build()
    {
        string projectFolder = Path.GetDirectoryName(Application.dataPath);
        string savePath = EditorUtility.SaveFilePanel("Build .apk file", projectFolder, "mr-remoting-android-app", "apk");
        if (string.IsNullOrWhiteSpace(savePath))
        {
            return;
        }
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = Scenes;
        buildPlayerOptions.locationPathName = savePath;
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.outputPath);
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.LogError($"Build {report.summary.result} with {report.summary.totalErrors} errors.");
        }
    }
}