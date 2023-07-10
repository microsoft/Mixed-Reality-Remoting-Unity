using System;
using System.IO;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Captures the ARCameraManager image and saves as a bmp file.
/// Image resolution is defined by ARFoundation (640 x 480)
/// </summary>
public class ARCameraCapture : MonoBehaviour
{
    [SerializeField] private ARCameraManager aRCameraManager;

    private string savePath;
    private bool isRecording;
    private int frameCount = 0;
    private int outputWidth;
    private int outputHeight;

    private void Start()
    {
        savePath = $"{Application.persistentDataPath}/CameraCapture";
    }

    public void StartRecording()
    {
        // Overwrite previous recordings
        if (Directory.Exists(savePath))
        {
            Directory.Delete(savePath, true);
        }
        Directory.CreateDirectory(savePath);

        Debug.Log("ARCameraCapture.StartRecording path: " + savePath);
        isRecording = true;
        frameCount = 0;
        aRCameraManager.frameReceived += OnCameraFrameReceived;
    }

    public void StopRecording()
    {
        Debug.Log("ARCameraCapture.StopRecording");
        aRCameraManager.frameReceived -= OnCameraFrameReceived;
        isRecording = false;
    }

    unsafe void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        if (!aRCameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            Debug.LogError("CameraCapture: Couldn't get latest cpu image");
            return;
        }

        if (!isRecording)
        {
            Debug.LogError("Not recording - shouldn't be subscribed to this event");
            return;
        }

        outputWidth = image.width;
        outputHeight = image.height;

        var conversionParams = new XRCpuImage.ConversionParams
        {
            inputRect = new RectInt(0, 0, image.width, image.height),
            outputDimensions = new Vector2Int(image.width, image.height),
            outputFormat = TextureFormat.RGB24,
            transformation = XRCpuImage.Transformation.MirrorX
        };

        try
        {
            // Allocate a buffer to store the image.
            int size = image.GetConvertedDataSize(conversionParams);
            var buffer = new NativeArray<byte>(size, Allocator.Temp);
            image.Convert(conversionParams, new IntPtr(buffer.GetUnsafePtr()), buffer.Length);

            // Save
            string path = $"{savePath}/{frameCount}.bmp";
            BitmapEncoder.WriteBitmapFile(path, outputWidth, outputHeight, buffer);
            frameCount++;
        }
        finally
        {
            image.Dispose();
        }
    }
}