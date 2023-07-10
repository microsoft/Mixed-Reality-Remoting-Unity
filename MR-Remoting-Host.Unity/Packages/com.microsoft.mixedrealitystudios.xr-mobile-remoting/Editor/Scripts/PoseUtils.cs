// Copyright (c) Microsoft Corporation. All rights reserved.

using UnityEngine;

namespace MobileHolographicRemoting
{
    using System;
    using UnityEngine;

    public static class PoseUtils
    {
        /// <summary>
        /// Converts a position and rotation in to a byte array
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static byte[] ToBytes(Vector3 position, Quaternion rotation)
        {
            int sizeOfVector = sizeof(float) * 3;

            byte[] bytes = new byte[sizeOfVector * 2];

            // Convert vector to bytes
            Buffer.BlockCopy(BitConverter.GetBytes(position.x), 0, bytes, 0 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(position.y), 0, bytes, 1 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(position.z), 0, bytes, 2 * sizeof(float), sizeof(float));

            // Convert quaternion euler to bytes
            Buffer.BlockCopy(BitConverter.GetBytes(rotation.eulerAngles.x), 0, bytes, 3 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(rotation.eulerAngles.y), 0, bytes, 4 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(rotation.eulerAngles.z), 0, bytes, 5 * sizeof(float), sizeof(float));

            return bytes;
        }

        public static Pose PoseFromBytes(byte[] bytes)
        {
            float px = BitConverter.ToSingle(bytes, 0 * sizeof(float));
            float py = BitConverter.ToSingle(bytes, 1 * sizeof(float));
            float pz = BitConverter.ToSingle(bytes, 2 * sizeof(float));

            float rx = BitConverter.ToSingle(bytes, 3 * sizeof(float));
            float ry = BitConverter.ToSingle(bytes, 4 * sizeof(float));
            float rz = BitConverter.ToSingle(bytes, 5 * sizeof(float));

            Vector3 position = new Vector3(px, py, pz);
            Quaternion rotation = Quaternion.Euler(rx, ry, rz);

            return new Pose(position, rotation);
        }
    }
}
