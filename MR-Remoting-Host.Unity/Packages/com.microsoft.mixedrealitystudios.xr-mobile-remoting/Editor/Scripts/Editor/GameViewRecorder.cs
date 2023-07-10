// Copyright (c) Microsoft Corporation. All rights reserved.

using System.IO;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using UnityEngine;

namespace MRMobileRemoting
{
    public class GameViewRecorder
    {
        private const float FrameRate = 25f;
        private RecorderController recorderController;

        public void StartRecording(string path, string cameraTag, int width, int height, bool overwrite)
        {
            Debug.Log($"GameViewRecorder.StartRecording path: {path}, cameraTag: {cameraTag}, width: {width}, height: {height}");
            
            if (overwrite && File.Exists(path))
            {
                Debug.Log("GameViewRecorder will overwrite video at path: " + path);
                File.Delete(path);
            }
            MovieRecorderSettings settings = ScriptableObject.CreateInstance<MovieRecorderSettings>();
            settings.name = "GameViewRecorder Settings";
            settings.Enabled = true;
            settings.OutputFormat = MovieRecorderSettings.VideoRecorderOutputFormat.MP4;
            settings.VideoBitRateMode = VideoBitrateMode.High;
            settings.AudioInputSettings.PreserveAudio = true;
            settings.ImageInputSettings = new CameraInputSettings
            {
                Source = ImageSource.TaggedCamera,
                CameraTag = cameraTag,
                OutputWidth = width,
                OutputHeight = height,
                FlipFinalOutput = true,
            };
            settings.OutputFile = path;

            var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
            recorderController = new RecorderController(controllerSettings);
            controllerSettings.AddRecorderSettings(settings);
            controllerSettings.SetRecordModeToManual();
            controllerSettings.FrameRate = FrameRate;
            controllerSettings.FrameRatePlayback = FrameRatePlayback.Constant;
            controllerSettings.CapFrameRate = true;

            RecorderOptions.VerboseMode = false;
            recorderController.PrepareRecording();
            recorderController.StartRecording();
        }

        public void StopRecording()
        {
            Debug.Log($"Stopped recording");
            recorderController.StopRecording();
        }
    }
}
