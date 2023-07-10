using Microsoft.MixedReality.WebRTC;
using PeerConnection = Microsoft.MixedReality.WebRTC.Unity.PeerConnection;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace MobileHolographicRemoting
{
    public class CameraPoseSource : MonoBehaviour
    {
        [Header("WebRTC settings")]
        [SerializeField] private PeerConnection peerConnection;
        [SerializeField] private ushort channelId = 0;

        [SerializeField] private ARSession arSession;

        [Header("The camera to send")]
        [SerializeField] private Camera cam;

        private DataChannel dataChannel;

        // The following two members are serialized so that they can viewed in the inspector
        [SerializeField] private Vector3 positionOffsetToSend;
        [SerializeField] private Vector3 rotationOffsetToSend;

        private void Start()
        {
            peerConnection.OnInitialized.AddListener(PeerConnection_OnInitialized);
        }

        private void Update()
        {
            positionOffsetToSend = cam.transform.position;
            rotationOffsetToSend = cam.transform.rotation.eulerAngles;

            if (dataChannel != null && dataChannel.State == DataChannel.ChannelState.Open)
            {
                byte[] bytes = PoseUtils.ToBytes(positionOffsetToSend, rotationOffsetToSend);
                dataChannel.SendMessage(bytes);
            }
        }

        #region PeerConnection events

        private async void PeerConnection_OnInitialized()
        {
            dataChannel = await peerConnection.Peer.AddDataChannelAsync(channelId, "Camera Pose", true, true);
        }

        #endregion

        #region Unity events

        /// <summary>
        /// When this is called, 
        /// </summary>
        public void OnResetMobileCameraInHost()
        {
            arSession.Reset();
#if UNITY_EDITOR
            cam.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
#endif
        }
        public void OnHoloLensScannedQR()
        {
            arSession.Reset();
#if UNITY_EDITOR
            cam.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
#endif
        }

        #endregion
    }
}