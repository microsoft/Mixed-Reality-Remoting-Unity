using Microsoft.MixedReality.QR;
using UnityEngine;

namespace MobileHolographicRemoting
{
    public class QrAlignment : MonoBehaviour
    {
        private QrCodeTracker qrCodeTracker;

        [SerializeField, Tooltip("Prefab to represent location of marker coordinate system")]
        private SpatialGraphNodeTracker spatialGraphNodePrefab;

        [Header("Events")]
        [SerializeField]
        private QrRecognisedEvent OnQrRecognised;

        // Instance of the spatial graph node tracker prefab
        private SpatialGraphNodeTracker spatialGraphNode;
        private bool isTracking;

        public void StartTracking()
        {
            Debug.Log("QrAlignment.StartTracking");

            gameObject.SetActive(true);

            if (qrCodeTracker == null)
            {
                qrCodeTracker = new QrCodeTracker();
                qrCodeTracker.OnQRAdded += OnQRAdded;
                qrCodeTracker.OnQRUpdated += OnQRUpdated;
                qrCodeTracker.OnQRRemoved += OnQRRemoved;
                qrCodeTracker.Initialise();
            }
            else
            {
                qrCodeTracker.StartTracking();

            }
            isTracking = true;
        }

        public void StopTracking()
        {
            Debug.Log("QrAlignment.StopTracking");
            qrCodeTracker.StopTracking();
            isTracking = false;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (isTracking)
            {
                qrCodeTracker.Update();
            }
        }

        private void OnCalibrate()
        {
            Debug.Log($"OnCalibrate called: position = [{spatialGraphNode.transform.position.x}, {spatialGraphNode.transform.position.y}, {spatialGraphNode.transform.position.z}]");
            var pose = new Pose(spatialGraphNode.transform.position, spatialGraphNode.transform.rotation);

            OnQrRecognised.Invoke(pose);
        }

        private void UpdateSpatialGraphNode(QRCode qrCode)
        {
            // TODO - can we do away with the SpatialGraphNode and do the pose calculations in this component?
            if (spatialGraphNode == null)
            {
                spatialGraphNode = Instantiate(spatialGraphNodePrefab, new Vector3(0, 0, 0), Quaternion.identity, transform);
                qrCodeTracker.TrackedObject = spatialGraphNode.gameObject;
            }
            spatialGraphNode.Id = qrCode.SpatialGraphNodeId;
            spatialGraphNode.PhysicalSideLength = qrCode.PhysicalSideLength;

            OnCalibrate();
        }

        #region QrCodeTracker events

        private void OnQRRemoved(QRCode qrCode)
        {
        }

        private void OnQRUpdated(QRCode qrCode)
        {
            Debug.Log("QrAlignment.OnQRUpdated " + qrCode.Data);

            UpdateSpatialGraphNode(qrCode);
        }

        private void OnQRAdded(QRCode qrCode)
        {
            Debug.Log("QrAlignment.OnQRAdded " + qrCode.Data);
            UpdateSpatialGraphNode(qrCode);
        }

        #endregion

#if UNITY_EDITOR

        [ContextMenu("Debug Find QR")]
        private void DebugFindQr()
        {
            // Create a pose 300mm in front of HoloLens cam
            Transform cam = Camera.main.transform;
            Vector3 pos = cam.position + (cam.forward * 0.3f);

            var pose = new Pose(pos, Quaternion.LookRotation(cam.forward));
            OnQrRecognised.Invoke(pose);
        }

#endif

    }
}