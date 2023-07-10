// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace MobileHolographicRemoting
{
    public static class UPMTools
    {
        private const string PathToPackage = "./Packages/com.microsoft.mixedrealitystudios.xr-mobile-remoting";

        private static PackRequest request;

        /// <summary>
        /// Display a save in folder.. UI prompt and create a UPM at the selected folder
        /// </summary>
        [MenuItem("Tools/XR Mobile Remoting/Export UPM Package (.tgz)")]
        public static void CreatePackageWithUI()
        {
            string releasesFolder = Path.GetFullPath(Path.Combine(Application.dataPath, "../Releases/"));
            string targetFolder = EditorUtility.OpenFolderPanel("Select a folder to export package..", releasesFolder, "");
            if (!string.IsNullOrEmpty(targetFolder))
            {
                CreatePackage(targetFolder);
            }
        }

        public static void CreatePackageCmdLine()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                Debug.Log("ARG " + i + ": " + args[i]);
                if (args[i] == "-TargetFolder")
                {
                    string targetFolder = args[i + 1];
                    CreatePackage(targetFolder);
                    return;
                }
            }

            Debug.LogError("-TargetFolder not set via command line argument - not creating the UPM package");
        }

        private static void CreatePackage(string targetFolder)
        {
            string packageFolder = Path.GetFullPath(PathToPackage);

            EditorApplication.update += EditorUpdate;
            request = Client.Pack(packageFolder, targetFolder);
        }

        private static void EditorUpdate()
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
            }
        }
    }
}