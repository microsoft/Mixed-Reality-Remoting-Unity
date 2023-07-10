// Copyright (c) Microsoft Corporation. All rights reserved.

using Microsoft.MixedReality.WebRTC;
using PeerConnection = Microsoft.MixedReality.WebRTC.Unity.PeerConnection;
using System;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.Events;

namespace MRMobileRemoting
{
    /// <summary>
    /// Ability to send and receive messages over a DataChannel.
    /// 
    /// This script must be replicated on both host/client projects (for now)
    /// </summary>
    public class MessageSourceAndReceiver : MonoBehaviour
    {
        [SerializeField] private PeerConnection peerConnection;
        [SerializeField] private ushort channelId = 1;

        [Header("Events")]
        [SerializeField] private UnityEvent onRefreshSnapshot;
        [SerializeField] private UnityEvent onStartRecording;
        [SerializeField] private UnityEvent onStopRecording;
        [SerializeField] private UnityEvent onAlignMobileCamWithHoloLensCam;
        [SerializeField] private UnityEvent onQrCodeUiShownOnMobile;
        [SerializeField] private UnityEvent onQrCodeUiHiddenOnMobile;
        [SerializeField] private UnityEvent onHoloLensScannedQR;

        private DataChannel dataChannel;
        private ConcurrentQueue<Message> messageQueue = new ConcurrentQueue<Message>();

        private void Start()
        {
            peerConnection.OnInitialized.AddListener(PeerConnection_OnInitialized);
        }

        private async void PeerConnection_OnInitialized()
        {
            dataChannel = await peerConnection.Peer.AddDataChannelAsync(channelId, "Message", true, true);
            dataChannel.MessageReceived += OnMessageRecieved;
        }

        /// <summary>
        /// Send a Message enum. Returns true if succeeded
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool Send(Message message)
        {
            if (dataChannel != null && dataChannel.State == DataChannel.ChannelState.Open)
            {
                byte[] bytes = BitConverter.GetBytes((int)message);
                dataChannel.SendMessage(bytes);
                return true;
            }

            Debug.LogError("MessageSource.Send did not send");
            return false;
        }

        [ContextMenu("SendRefresh")]
        public bool SendRefreshCamera()
        {
            return Send(Message.RefreshSnapshot);
        }

        [ContextMenu("AlignMobileCamWithHoloLensCam")]
        public void SendAlignMobileCamWithHoloLensCam()
        {
            Send(Message.AlignMobileCamWithHoloLensCam);
        }

        public void SendHoloLensScannedQR()
        {
            Send(Message.HoloLensScannedQR);
        }

        private void Update()
        {
            while (!messageQueue.IsEmpty)
            {
                if (messageQueue.TryDequeue(out Message message))
                {
                    HandleMessageReceived(message);
                }
            }
        }

        private void HandleMessageReceived(Message message)
        {
            Debug.Log("Received message " + message);

            if (message == Message.RefreshSnapshot)
            {
                onRefreshSnapshot?.Invoke();
            }
            else if (message == Message.StartRecording)
            {
                onStartRecording?.Invoke();
            }
            else if (message == Message.StopRecording)
            {
                onStopRecording?.Invoke();
            }
            else if (message == Message.AlignMobileCamWithHoloLensCam)
            {
                onAlignMobileCamWithHoloLensCam?.Invoke();
            }
            else if (message == Message.HoloLensScannedQR)
            {
                onHoloLensScannedQR?.Invoke();
            }
            else if (message == Message.QrCodeUiShownOnMobile)
            {
                onQrCodeUiShownOnMobile?.Invoke();
            }
            else if (message == Message.QrCodeUiHiddenOnMobile)
            {
                onQrCodeUiHiddenOnMobile?.Invoke();
            }
            else
            {
                throw new ArgumentException("Unknown message received: " + message);
            }
        }

        #region DataChannel events

        private void OnMessageRecieved(byte[] bytes)
        {
            if (bytes.Length != sizeof(int))
            {
                throw new ArgumentException("MessageReceiver should only be receiving integers");
            }
            int intMessage = BitConverter.ToInt32(bytes, 0);
            Message message = (Message)intMessage;
            messageQueue.Enqueue(message);
        }

        #endregion
    }
}
