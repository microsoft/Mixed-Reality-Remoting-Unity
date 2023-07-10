// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Events;

namespace MRMobileRemoting
{
    [Serializable]
    public class QrRecognisedEvent : UnityEvent<Pose> { }
}