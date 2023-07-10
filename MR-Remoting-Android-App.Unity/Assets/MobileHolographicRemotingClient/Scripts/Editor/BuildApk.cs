using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;
using System;
using System.IO;

public class BuildApk
{
    static readonly string[] Scenes = {
        "Assets/MobileHolographicRemotingClient/Scenes/main.unity"
    };

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

    [MenuItem("Build APK/Build")]
    static void Build()
    {
        string debugPath = "C:/Program Files/Unity/Hub/Editor/2020.3.20f1/Editor/Data/PlaybackEngines/AndroidPlayer";
        foreach (string file in Directory.EnumerateFiles(debugPath, "*.*", SearchOption.AllDirectories))
        {
            Debug.Log(file);
        }

        string targetFolder = GetTargetFolder();
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = Scenes;
        buildPlayerOptions.locationPathName = Path.Combine(targetFolder, "build.apk");
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
            EditorApplication.Exit(1);
        }
    }
}