// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using Microsoft.MixedReality.WebRTC;
using UnityEngine;
using UnityEngine.Events;
using PeerConnection = Microsoft.MixedReality.WebRTC.Unity.PeerConnection;

namespace MRMobileRemoting
{
    [Serializable]
    public class TextureReceivedEvent : UnityEvent<Texture2D> { }

    /// <summary>
    /// Sets up receiving a message from a PeerConnection
    /// </summary>
    public class TextureReceiver : MonoBehaviour
    {
        [SerializeField] private PeerConnection peerConnection;
        [SerializeField] private ushort channelId = 2;

        [Header("Events")]
        [SerializeField] private TextureReceivedEvent OnTextureReceived;

        private DataChannel dataChannel;
        private byte[] textureReceivedBytes;
        private int textureExpectedSize;
        private int textureReceivedSize;

        private bool isReadyToProcessTexture;

        public Texture2D mostRecentTexture;

        private void Start()
        {
            peerConnection.OnInitialized.AddListener(PeerConnection_OnInitialized);
            mostRecentTexture = new Texture2D(1, 1);
        }

        private async void PeerConnection_OnInitialized()
        {
            dataChannel = await peerConnection.Peer.AddDataChannelAsync(channelId, "Camera Pose", true, true);
            dataChannel.MessageReceived += OnMessageRecieved;
        }

        private void Update()
        {
            if (isReadyToProcessTexture)
            {
                mostRecentTexture.LoadImage(textureReceivedBytes, false);
                mostRecentTexture.Apply();

                OnTextureReceived?.Invoke(mostRecentTexture);

                isReadyToProcessTexture = false;
                textureExpectedSize = 0;
            }
        }

        #region DataChannel events

        private void OnMessageRecieved(byte[] bytes)
        {
            // It appears that textures are sent in chunks. On the sender side, I initially send a int containing the size of the texture (i.e num bytes)
            // So first thing to check is if the first byte array is an int.
            if (bytes.Length == sizeof(int))
            {
                if (textureExpectedSize != 0)
                {
                    Debug.LogError("Likely in the process of receiving another texture. Will now process the most recently received");
                }

                textureExpectedSize = BitConverter.ToInt32(bytes, 0);
                textureReceivedSize = 0;
                textureReceivedBytes = new byte[textureExpectedSize];

                Debug.Log("Will receive texture with size: " + textureExpectedSize);
            }
            // Everything else should be chunked bytes of a texture. Use BlockCopy to copy in to an array
            // and process once we've received all bytes
            else
            {
                Buffer.BlockCopy(bytes, 0, textureReceivedBytes, textureReceivedSize, bytes.Length);
                textureReceivedSize += bytes.Length;

                Debug.Log($"Received bytes: {textureReceivedSize}. Expected: {textureExpectedSize}");

                // We've now received all bytes. Ready to process an image on the main thread on the next Update
                if (textureReceivedSize == textureExpectedSize)
                {
                    isReadyToProcessTexture = true;
                }
            }
        }

        #endregion
    }
}
