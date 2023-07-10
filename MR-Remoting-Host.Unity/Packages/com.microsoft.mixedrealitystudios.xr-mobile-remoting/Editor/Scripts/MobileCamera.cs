// Copyright (c) Microsoft Corporation. All rights reserved.

using UnityEngine;

namespace MobileHolographicRemoting
{
    /// <summary>
    /// Component attached to the camera that represents the AR mobile phone in this scene's world.
    /// </summary>
    public class MobileCamera : MonoBehaviour
    {
        private const string StreamResTip = "Changes resolution of camera used for STREAMING PREVIEW, as displayed on the mobile app. Higher quality will take longer to transmit over webRTC";
        private const string StreamCamTip = "The camera used for steaming images to the mobile app";

        #region Inspector fields

        [Tooltip(StreamResTip)]
        public CameraResolution.Size StreamResolution = CameraResolution.Size.SD_480;

        [Tooltip(StreamCamTip)]
        public Camera StreamCamera;

        #endregion

        private RenderTexture streamRT;
        private RenderTexture recordingRT;

        public void ResetStreamRenderTexture()
        {
            SetRenderTextureResolution(StreamCamera.targetTexture, StreamResolution);
        }

        private static void SetRenderTextureResolution(RenderTexture renderTexture, CameraResolution.Size resolution)
        {
            CameraResolution.GetDimensions(resolution, out int width, out int height);

            renderTexture.Release();
            renderTexture.width = width;
            renderTexture.height = height;
            renderTexture.Create();
        }

        #region Unity event

        /// <summary>
        /// This sets the inital position and rotation members vars to the current position
        /// and rotation of the HoloLens
        /// 
        /// Received from the MessageSourceAndReceiver component
        /// </summary>
        public void OnAlignMobileCamWithHoloLensCam()
        {
            // Set the inital position and rotation to match the hololens cam
            Transform hl = Camera.main.transform;
            transform.SetPositionAndRotation(hl.position, hl.rotation);
        }

        /// <summary>
        /// Takes the pose and adds to the inital position and rotation
        /// 
        /// Received from the CameraPoseReceiver component
        /// </summary>
        /// <param name="pose"></param>
        public void OnReceivedCameraPose(Pose pose)
        {
            StreamCamera.transform.localRotation = pose.rotation;
            StreamCamera.transform.localPosition = pose.position;
        }

        /// <summary>
        /// Positions and orientates the cameras to match a qr code pose
        /// </summary>
        /// <param name="qrPose"></param>
        public void OnQrRecognised(Pose qrPose)
        {
            // Set x and z components of the rotation to 0, as this is what happens to the phone's rotation when ARSession.Reset is calleds
            Vector3 rotation = qrPose.rotation.eulerAngles;
            rotation.x = 0f;
            rotation.z = 0f;
            transform.SetPositionAndRotation(qrPose.position, Quaternion.Euler(rotation));
        }

        #endregion

        private void OnValidate()
        {
            SetRenderTextureResolution(StreamCamera.targetTexture, StreamResolution);
        }
    }
}
