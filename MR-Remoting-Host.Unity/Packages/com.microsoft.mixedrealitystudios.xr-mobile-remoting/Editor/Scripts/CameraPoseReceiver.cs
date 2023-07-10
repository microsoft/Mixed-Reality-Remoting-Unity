// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Concurrent;
using Microsoft.MixedReality.WebRTC;
using UnityEngine;
using UnityEngine.Events;
using PeerConnection = Microsoft.MixedReality.WebRTC.Unity.PeerConnection;

namespace MobileHolographicRemoting
{
    [Serializable]
    public class CameraPoseReceivedEvent : UnityEvent<Pose> { }

    /// <summary>
    /// Sets up receiving a message from a PeerConnection
    /// </summary>
    public class CameraPoseReceiver : MonoBehaviour
    {
        [SerializeField] private PeerConnection peerConnection;
        [SerializeField] private ushort channelId = 0;

        [Header("Events")]
        [SerializeField] private CameraPoseReceivedEvent OnReceievedPose;

        private DataChannel dataChannel;
        private ConcurrentQueue<Pose> poseQueue = new ConcurrentQueue<Pose>();

        private void Start()
        {
            peerConnection.OnInitialized.AddListener(PeerConnection_OnInitialized);
        }

        private async void PeerConnection_OnInitialized()
        {
            dataChannel = await peerConnection.Peer.AddDataChannelAsync(channelId, "Camera Pose", true, true);
            dataChannel.MessageReceived += OnMessageRecieved;
        }

        private void Update()
        {
            while (!poseQueue.IsEmpty)
            {
                if (poseQueue.TryDequeue(out Pose pose))
                {
                    OnReceievedPose?.Invoke(pose);
                }
            }
        }

        #region DataChannel events

        private void OnMessageRecieved(byte[] bytes)
        {
            Pose pose = PoseUtils.PoseFromBytes(bytes);
            poseQueue.Enqueue(pose);
        }

        #endregion
    }
}
