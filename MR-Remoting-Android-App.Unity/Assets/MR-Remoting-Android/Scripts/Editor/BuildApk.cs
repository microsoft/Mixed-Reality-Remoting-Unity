using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;
using System.IO;
using System;

public class BuildApk
{
    static readonly string[] Scenes = {
        "Assets/MR-Remoting-Android/Scenes/main.unity"
    };

    static readonly string ApkFileName = $"mr-mobile-remoting-android-app-{Application.version}";

    static string GetTargetFolder()
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-TargetFolder")
            {
                string targetFolder = args[i + 1];
                return targetFolder;
            }
        }

        Debug.LogError("-TargetFolder not set via command line argument");
        return string.Empty;
    }

    static string GetBuildFilePath()
    {
        string filePath = string.Empty;

        if (Application.isBatchMode)
        {
            string commandLineTargetFolder = GetTargetFolder();
            if (!string.IsNullOrEmpty(commandLineTargetFolder))
            {
                filePath = Path.Combine(commandLineTargetFolder, ApkFileName);
            }
        }
        else
        {
            string projectFolder = Path.GetDirectoryName(Application.dataPath);
            filePath = EditorUtility.SaveFilePanel("Build .apk file", projectFolder, ApkFileName, "apk");
        }
        return filePath;
    }

    [MenuItem("Tools/Build MR Remoting Android APK")]
    static void Build()
    {
        string filePath = GetBuildFilePath();
        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = Scenes;
        buildPlayerOptions.locationPathName = filePath;
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