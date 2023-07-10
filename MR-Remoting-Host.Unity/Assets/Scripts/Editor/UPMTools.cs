// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace MRMobileRemoting
{
    public static class UPMTools
    {
        private const string PathToPackage = "./Packages/com.microsoft.mixedrealitystudios.xr-mobile-remoting";

        private static PackRequest request;

        [MenuItem("Tools/MR Mobile Remoting/Export UPM Package (.tgz)")]
        static void CreatePackage()
        {
            string path = GetBuildPath();

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            string packageFolder = Path.GetFullPath(PathToPackage);
            EditorApplication.update += EditorUpdate;
            request = Client.Pack(packageFolder, path);
        }

        static string GetBuildPath()
        {
            string path = string.Empty;
            if (Application.isBatchMode)
            {
                path = GetTargetFolder();
            }
            else
            {
                string releasesFolder = Path.GetFullPath(Path.Combine(Application.dataPath, "../"));
                path = EditorUtility.OpenFolderPanel("Select a folder to export package..", releasesFolder, "");
            }

            return path;
        }

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

        static void EditorUpdate()
        {
            if (request.IsCompleted)
            {
                if (request.Status == StatusCode.Success)
                {
                    Debug.Log("Saved package to: " + request.Result.tarballPath);
                }
                else
                {
                    Debug.LogWarning("An error occurred when creating package");
                }
                EditorApplication.update -= EditorUpdate;

                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(0);
                }
            }
        }
    }
}