using Microsoft.MixedReality.QR;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MRMobileRemoting
{
    public delegate void QRCodeFunction(QRCode qrCode);

    internal class QrCodeTracker
    {
        private DateTime _watcherStart;

        //private TransformListener _listener;
        private QRCodeWatcher _qrWatcher = null;
        private bool _watching;
        private bool _calibrating;

        public event EventHandler OnPinned;

        /// <summary>
        /// Callback when a new QR code is added.
        /// </summary>
        public event QRCodeFunction OnQRAdded;

        /// <summary>
        /// Callback when a previously added QR code is updated.
        /// </summary>
        public event QRCodeFunction OnQRUpdated;

        /// <summary>
        /// Callback when a previously added QR code is removed.
        /// </summary>
        public event QRCodeFunction OnQRRemoved;

        /// <summary>
        /// Callback when the enumeration is complete.
        /// </summary>
        /// <remarks>
        /// Cached QR codes will have Added and Updated events BEFORE the enumeration complete.
        /// Newly seen QR codes will only start to appear after the enumeration complete event.
        /// <see href="https://github.com/chgatla-microsoft/QRTracking/issues/2"/>
        /// </remarks>
        public event QRCodeFunction OnQREnumerated;

        /// <summary>
        /// Events are stored in the PendingQRCode struct for re-issue on the main thread.
        /// </summary>
        /// <remarks>
        /// While more elegant mechanisms exist for accomplishing the same thing, the simplicity of
        /// this form provides great efficiency, especially for memory pressure.
        /// </remarks>
        private struct PendingQRCode
        {
            /// <summary>
            /// The four actions that can be taken, corresponding to the 4 subscribable delegates.
            /// </summary>
            public enum QRAction
            {
                Add,
                Update,
                Remove,
                Enumerated
            };

            /// <summary>
            /// The code which has triggered the event. For Enumerated action, qrCode will be null.
            /// </summary>
            public readonly QRCode qrCode;

            /// <summary>
            /// The type of event.
            /// </summary>
            public readonly QRAction qrAction;

            /// <summary>
            /// Constructor for immutable action.
            /// </summary>
            /// <param name="qrAction">Action to take.</param>
            /// <param name="qrCode">QR Code causing event.</param>
            public PendingQRCode(QRAction qrAction, QRCode qrCode)
            {
                this.qrAction = qrAction;
                this.qrCode = qrCode;
            }
        }

        /// <summary>
        /// Queue of qr code events to process next Update.
        /// </summary>
        private readonly Queue<PendingQRCode> pendingActions = new Queue<PendingQRCode>();

        public bool IsTracking { get => _watching; }

        public GameObject TrackedObject { get; set; }

        public async void Initialise()
        {
            if (QRCodeWatcher.IsSupported())
            {
                // This will never run as we are only running from the Unity editor
#if ENABLE_WINMD_SUPPORT
            try
            {
                var capture = new global::Windows.Media.Capture.MediaCapture();
                await capture.InitializeAsync();
                Debug.Log("Camera and Microphone permissions OK");
            }
            catch (UnauthorizedAccessException)
            {
                Debug.LogError("Camera and microphone permissions not granted.");
                return;
            }
#endif

                if (await QRCodeWatcher.RequestAccessAsync() == QRCodeWatcherAccessStatus.Allowed)
                {
                    Debug.Log("Access allowed: configuring QR Code Watcher");

                    _watcherStart = DateTime.Now;
                    _qrWatcher = new QRCodeWatcher();
                    _qrWatcher.Added += OnQRCodeAddedEvent;
                    _qrWatcher.Updated += OnQRCodeUpdatedEvent;
                    _qrWatcher.Removed += OnQRCodeRemovedEvent;
                    _qrWatcher.EnumerationCompleted += OnQREnumerationEnded;
                    _qrWatcher.Start();
                }
            }
        }

        public void Calibrate()
        {
            if (!_calibrating)
            {
                _calibrating = true;
                StartTracking();
            }
            else
            {
                _calibrating = false;

                OnPinned?.Invoke(this, new EventArgs());
            }
        }

        public void StartTracking()
        {
            if (!_watching)
            {
                _watching = true;
                _qrWatcher?.Start();
                Debug.Log("Tracking Started");
            }
        }

        public void StopTracking()
        {
            if (_watching)
            {
                _watching = false;
                _qrWatcher?.Stop();
                Debug.Log("Tracking Stopped");
            }
        }

        /// <summary>
        /// Lazily create qr code watcher resources if needed, then issue any queued events.
        /// </summary>
        public void Update()
        {
            lock (pendingActions)
            {
                while (pendingActions.Count > 0)
                {
                    var action = pendingActions.Dequeue();

                    switch (action.qrAction)
                    {
                        case PendingQRCode.QRAction.Add:
                            OnQRAdded?.Invoke(action.qrCode);
                            break;

                        case PendingQRCode.QRAction.Update:
                            OnQRUpdated?.Invoke(action.qrCode);
                            break;

                        case PendingQRCode.QRAction.Remove:
                            OnQRRemoved?.Invoke(action.qrCode);
                            break;

                        case PendingQRCode.QRAction.Enumerated:
                            OnQREnumerated?.Invoke(null);
                            break;

                        default:
                            Debug.Assert(false, "Unknown action type");
                            break;
                    }
                }
            }

        }

        /// <summary>
        /// Capture an Added event for later call on main thread.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="args">Args containing relevant QRCode.</param>
        private void OnQRCodeAddedEvent(object sender, QRCodeAddedEventArgs args)
        {
            Debug.Log($"QRCodeAdded: {args.Code.Id}");

            if (args.Code.LastDetectedTime < _watcherStart)
                return;
            lock (pendingActions)
            {
                pendingActions.Enqueue(new PendingQRCode(PendingQRCode.QRAction.Add, args.Code));
            }
        }

        /// <summary>
        /// Capture an Updated event for later call on main thread.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="args">Args containing relevant QRCode.</param>
        private void OnQRCodeUpdatedEvent(object sender, QRCodeUpdatedEventArgs args)
        {
            Debug.Log($"QRCodeUpdated: {args.Code.Id}");

            if (args.Code.LastDetectedTime < _watcherStart)
                return;
            lock (pendingActions)
            {
                pendingActions.Enqueue(new PendingQRCode(PendingQRCode.QRAction.Update, args.Code));
            }
        }

        /// <summary>
        /// Capture a Removed event for later call on main thread.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="args">Args containing relevant QRCode.</param>
        private void OnQRCodeRemovedEvent(object sender, QRCodeRemovedEventArgs args)
        {
            Debug.Log($"QRCodeRemoved: {args.Code.Id}");

            if (args.Code.LastDetectedTime < _watcherStart)
                return;
            lock (pendingActions)
            {
                pendingActions.Enqueue(new PendingQRCode(PendingQRCode.QRAction.Remove, args.Code));
            }
        }

        /// <summary>
        /// Capture the Enumeration Ended event for later call on main thread.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void OnQREnumerationEnded(object sender, object e)
        {
            lock (pendingActions)
            {
                pendingActions.Enqueue(new PendingQRCode(PendingQRCode.QRAction.Enumerated, null));
            }
        }
    }
}