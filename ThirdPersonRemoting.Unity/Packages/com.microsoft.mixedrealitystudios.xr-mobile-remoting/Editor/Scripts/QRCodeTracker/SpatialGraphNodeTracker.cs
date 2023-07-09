using Microsoft.MixedReality.OpenXR;
using UnityEngine;

namespace MobileHolographicRemoting
{
    public class SpatialGraphNodeTracker : MonoBehaviour
    {
        private System.Guid _id;
        private SpatialGraphNode node;

        public System.Guid Id
        {
            get => _id;

            set
            {
                if (_id != value)
                {
                    _id = value;
                    InitializeSpatialGraphNode(force: true);
                }
            }
        }

        public float PhysicalSideLength { get; internal set; }

        private Camera mainCam;

        // Use this for initialization
        void Start()
        {
            mainCam = Camera.main;
            InitializeSpatialGraphNode();
        }

        // Update is called once per frame
        void Update()
        {
            InitializeSpatialGraphNode();
            if (node != null && node.TryLocate(FrameTime.OnUpdate, out Pose pose))
            {
                // If there is a parent to the camera that means we are using teleport and we should not apply the teleport
                // to these objects so apply the inverse

                if (mainCam.transform.parent != null)
                {
                    pose = pose.GetTransformedBy(mainCam.transform.parent);
                }

                // Move the anchor point to the *center* of the QR code
                var deltaToCenter = PhysicalSideLength * 0.5f;
                pose.position += (pose.rotation * (deltaToCenter * Vector3.right) -
                                  pose.rotation * (deltaToCenter * Vector3.down));

                // If we want to keep the discovered position/rotation axis-aligned we can do something like
                // the following.
                pose.rotation = Quaternion.Euler(pose.rotation.eulerAngles.x, pose.rotation.eulerAngles.y, pose.rotation.eulerAngles.z)
                    * Quaternion.Euler(180, 0, 0);

                gameObject.transform.SetPositionAndRotation(pose.position, pose.rotation);
                //Debug.Log("Id= " + id + " QRPose = " +  pose.position.ToString("F7") + " QRRot = "  +  pose.rotation.ToString("F7"));
            }
        }

        private void InitializeSpatialGraphNode(bool force = false)
        {
            if (node == null || force)
            {
                node = (Id != System.Guid.Empty) ? SpatialGraphNode.FromStaticNodeId(Id) : null;
                //Debug.Log("Initialize SpatialGraphNode Id= " + Id);
            }
        }
    }
}