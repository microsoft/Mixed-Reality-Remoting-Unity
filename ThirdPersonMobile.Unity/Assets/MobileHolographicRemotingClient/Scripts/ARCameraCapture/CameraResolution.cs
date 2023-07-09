// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace MobileHolographicRemoting
{
    public static class CameraResolution
    {
        private const float Aspect = 16f / 9f;

        /// <summary>
        /// The y-component of screen resolutions. Multiply by aspect to obtain x component, i.e x = y * 16/9
        /// </summary>
        public enum Size
        {
            SD_480,
            HD_720,
            FHD_1080,
            QHD,
            FourK,
        }

        private static Dictionary<string, Size> LabelLookup = new Dictionary<string, Size>()
        {
            { "SD - 480p", Size.SD_480 },
            { "HD - 720p", Size.HD_720 },
            { "FHD - 1080p", Size.FHD_1080 },
            { "QHD - 1440p", Size.QHD },
            { "4K - 2160p", Size.FourK },
        };

        private static Dictionary<Size, int> SizeLookup = new Dictionary<Size, int>()
        {
            { Size.SD_480, 480},
            { Size.HD_720, 720},
            { Size.FHD_1080, 1080},
            { Size.QHD, 1440},
            { Size.FourK, 2160},
        };

        public static string[] Labels = LabelLookup.Keys.ToArray();

        /// <summary>
        /// Get dimensions using default aspect (16:9)
        /// </summary>
        /// <param name="size"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void GetDimensions(Size size, out int width, out int height)
        {
            GetDimensions(size, Aspect, out width, out height);
        }

        /// <summary>
        /// Get dimensions using an arbritary aspect
        /// </summary>
        /// <param name="size"></param>
        /// <param name="aspect"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void GetDimensions(Size size, float aspect, out int width, out int height)
        {
            height = SizeLookup[size];
            width = Mathf.RoundToInt(height * aspect);
        }
    }
}
