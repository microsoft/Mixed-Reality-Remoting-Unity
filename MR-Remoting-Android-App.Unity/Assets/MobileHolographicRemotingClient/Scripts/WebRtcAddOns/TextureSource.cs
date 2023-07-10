using System;
using Microsoft.MixedReality.WebRTC;
using UnityEngine;
using PeerConnection = Microsoft.MixedReality.WebRTC.Unity.PeerConnection;

namespace MobileHolographicRemoting
{
    /// <summary>
    /// Component used for taking a snapshot from a camera and then sending it over a DataChannel
    /// </summary>
    public class TextureSource : MonoBehaviour
    {
        // The resolution of the image to send (height will be scaled according to aspect)
        private const int TextureWidth = 480;

        // Texture format must be the same on the receiver
        private const TextureFormat Format = TextureFormat.RGB565;

        [SerializeField] private PeerConnection peerConnection;
        [SerializeField] private ushort channelId = 2;

        [Header("The camera to send")]
        [SerializeField] private Camera cam;

        private DataChannel dataChannel;

        bool isSending = true;

        public Texture2D snapshot;

        private void Start()
        {
            peerConnection.OnInitialized.AddListener(PeerConnection_OnInitialized);
        }

        public void SendCameraImage()
        {
            Camera.onPostRender += OnCameraPostRender;
            isSending = true;
        }

        #region PeerConnection events

        private async void PeerConnection_OnInitialized()
        {
            dataChannel = await peerConnection.Peer.AddDataChannelAsync(channelId, "Camera Image", true, true);
        }

        #endregion

        #region Camera events

        private void OnCameraPostRender(Camera camera)
        {
            if (!isSending || camera != cam)
            {
                return;
            }

            if (snapshot == null)
            {
                snapshot = new Texture2D(camera.pixelWidth, camera.pixelHeight, TextureFormat.RGB565, false);
            }

            Rect rect = new Rect(0, 0, camera.pixelWidth, camera.pixelHeight);
            snapshot.ReadPixels(rect, 0, 0, false);
            snapshot.Apply();

            byte[] bytes = snapshot.EncodeToJPG();
            dataChannel.SendMessage(BitConverter.GetBytes(bytes.Length));   // Send number of bytes so receiver knows how how many chunks
            dataChannel.SendMessage(bytes);

            Debug.Log("Sending camera image " + bytes.Length);

            Camera.onPostRender -= OnCameraPostRender;
            isSending = false;
        }

        #endregion
    }
}