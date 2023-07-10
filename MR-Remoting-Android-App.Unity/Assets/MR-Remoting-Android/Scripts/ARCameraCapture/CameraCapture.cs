using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

namespace MobileHolographicRemoting
{
    /// <summary>
    /// Captures a camera's image to file. Based on https://gist.github.com/DashW/74d726293c0d3aeb53f4
    /// Supports multiple resolution outputs and Start/Stop recording
    /// </summary>
    public class CameraCapture : MonoBehaviour
    {
        [SerializeField]
        private Camera targetCamera;

        [SerializeField, Tooltip("The resolution of the saved image. Higher resolutions require more processing and memory.")]
        private CameraResolution.Size resolution = CameraResolution.Size.SD_480;

        [SerializeField]
        private bool clearSaveDirectoryOnRecord = true;

        #region Events

        private event Action<string> OnRecordingStopped;

        #endregion

        private string savePath;
        private int outputWidth;
        private int outputHeight;
        private Queue<byte[]> frameQueue = new Queue<byte[]>();
        private Texture2D tempTexture;
        private Rect captureRect;
        private int frameCount = 0;

        private Thread encoderThead;
        private bool isCapturing;

        [ContextMenu("Start Recording")]
        public void StartRecording()
        {
            if (isCapturing && encoderThead.IsAlive)
            {
                Debug.LogError("CameraCapture is already in recording mode");
                return;
            }

            if (clearSaveDirectoryOnRecord)
            {
                Directory.Delete(savePath, true);
                Directory.CreateDirectory(savePath);
            }

            frameCount = 0;
            isCapturing = true;
            encoderThead = new Thread(EncodeAndSave);
            encoderThead.Start();
        }

        [ContextMenu("Stop Recording")]
        public void StopRecording()
        {
            if (isCapturing)
            {
                isCapturing = false;
                StartCoroutine(InvokeEventWhenThreadEnds());
            }
        }

        private void Start()
        {
            savePath = $"{Application.persistentDataPath}/CameraCapture";
            Directory.CreateDirectory(savePath);

            CameraResolution.GetDimensions(resolution, targetCamera.aspect, out outputWidth, out outputHeight);
            
            tempTexture = new Texture2D(outputWidth, outputHeight, TextureFormat.RGB24, false);
            captureRect = new Rect(0, 0, targetCamera.pixelWidth, targetCamera.pixelHeight);

            Camera.onPostRender += OnCameraPostRender;

            Debug.Log($"CameraCapture path: {savePath}, width: {outputWidth}, height: {outputHeight}");
            Debug.Log($"Camera pixel width: {targetCamera.pixelWidth}, height: {targetCamera.pixelHeight}. Screen width: {Screen.width}, height: {Screen.height}");
        }

        private void OnDisable()
        {
            isCapturing = false;
            Camera.onPostRender -= OnCameraPostRender;
        }

        private void OnApplicationPause()
        {
            if (isCapturing)
            {
                Debug.Log("Application paused - stopping recording");
                isCapturing = false;
            }
        }

        private void OnCameraPostRender(Camera cam)
        {
            if (cam != targetCamera || !isCapturing)
            {
                return;
            }

            // RenderTexture.active = cam.activeTexture;
            tempTexture.ReadPixels(captureRect, 0, 0);
            // RenderTexture.active = null;

            frameQueue.Enqueue(tempTexture.GetRawTextureData());
        }

        private IEnumerator InvokeEventWhenThreadEnds()
        {
            yield return new WaitUntil(() =>
            {
                Debug.Log("CameraCapture - is thread still alive? " + encoderThead.IsAlive);
                return !encoderThead.IsAlive;
            });
            OnRecordingStopped?.Invoke(savePath);
        }

        private void EncodeAndSave()
        {
            while (isCapturing)
            {
                if (frameQueue.Count > 0)
                {
                    string path = $"{savePath}/{frameCount}.bmp";
                    BitmapEncoder.WriteBitmapFile(path, outputWidth, outputHeight, frameQueue.Dequeue());
                    frameCount++;

                    Debug.Log($"CameraCamera saved frame: {path}. Queue count: {frameQueue.Count}");
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }
    }
}