// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace MobileHolographicRemoting
{
    /// <summary>
    /// Editor window for third person remoting
    /// </summary>
    public class MRMobileRemotingEditor : EditorWindow
    {
        /// <summary>
        /// Window display config
        /// </summary>
        private const float MaxWidthCameraImage = 480;
        private const string WindowTitle = "XR Mobile Remoting";
        private const string TagRecordingCamera = "RecordingCamera";

        /// <summary>
        /// Path to main prefab
        /// </summary>
        private const string PrefabPath = "Packages/com.microsoft.mixedrealitystudios.xr-mobile-remoting/Editor/MobileRemotingHost.prefab";

        /// <summary>
        /// Editor pref keys
        /// </summary>
        private const string PrefsShowGettingStarted = "Remoting_ShowGettingStarted";
        private const string PrefsShowConnection = "Remoting_ShowConnection";
        private const string PrefsServerPath = "Remoting_ServerPath";
        private const string PrefsFilename = "Remoting_Filename";

        private MobileCamera mobileCamera;
        private MessageSourceAndReceiver messageSourceAndReceiver;
        private TextureReceiver textureReceiver;
        private string recordingFilename;
        private string ipAddress;
        private Vector2 scrollPosition;
        private bool isRecording;
        private bool didInitialiseSuccessfully;
        private GameViewRecorder gameViewRecorder;

        [MenuItem("Window/MR Mobile Remoting")]
        static void Init()
        {
            MRMobileRemotingEditor window = (MRMobileRemotingEditor)EditorWindow.GetWindow(typeof(MRMobileRemotingEditor), false, WindowTitle);
            window.Show();
        }

        private void OnEnable()
        {
            InitialiseReferences();
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            if (obj == PlayModeStateChange.EnteredPlayMode)
            {
                InitialiseReferences();
            }

            if (!didInitialiseSuccessfully)
            {
                Debug.LogError("MRMobileRemotingEditor did not initialise properly");
            }
        }

        private void InitialiseReferences()
        {
            mobileCamera = FindObjectOfTypeWithError<MobileCamera>();
            textureReceiver = FindObjectOfTypeWithError<TextureReceiver>();
            messageSourceAndReceiver = FindObjectOfTypeWithError<MessageSourceAndReceiver>();
            ipAddress = GetLocalIPAddress();

            didInitialiseSuccessfully = mobileCamera != null &&
                textureReceiver != null &&
                messageSourceAndReceiver != null &&
                !string.IsNullOrEmpty(ipAddress);

        }

        private void OnGUI()
        {
            if (!didInitialiseSuccessfully)
            {
                DrawSectionInstallComponents();
                return;
            }

            EditorGUIUtility.labelWidth = 200;
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            DrawSectionInstructions();
            DrawSectionConnection();
            DrawSectionCameraFeeds();
            DrawSectionRecording();
            EditorGUILayout.EndScrollView();
        }

        private void DrawSectionInstallComponents()
        {
            EditorGUILayout.LabelField("Scene Requires Additional Components", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Click the button below to add a prefab containing the required components to enable an XR Mobile remoting session");
            if (GUILayout.Button("Install Components in to current scene", GUILayout.Width(250)))
            {
                GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath(PrefabPath, typeof(GameObject));
                PrefabUtility.InstantiatePrefab(prefab);
                InitialiseReferences();
            }
        }

        private void DrawSectionInstructions()
        {
            bool showFoldout = EditorPrefs.GetBool(PrefsShowGettingStarted, true);
            showFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(showFoldout, "Getting Started - READ ME FIRST!");

            var richText = new GUIStyle(EditorStyles.label)
            {
                richText = true
            };

            if (showFoldout)
            {
                EditorGUILayout.LabelField("This tool enables a mobile phone to join and a Holographic Remoting session");
                EditorGUILayout.Space(10);

                EditorGUILayout.LabelField("One-time setup", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("1. If using holographic remoting, platform-switch this project to UWP and enable remoting in the XR Plug-in Management settings");
                EditorGUILayout.LabelField("2. Ensure this PC, HoloLens and Phone are on the same network. A mobile hotspot works fine.");
                EditorGUILayout.LabelField("3. Install the mobile companion app on an Android phone");
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Session setup", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("1. Run the WebRTC signalling server running on this PC. You can use the tool below");
                EditorGUILayout.LabelField("2. (Optional) Connect the HoloLens to Holographic Remoting. Window > XR > Holographic Remoting for Play Mode");
                EditorGUILayout.LabelField("3. Hit play mode in this Editor");
                EditorGUILayout.LabelField("4. On mobile app: Enter this computer's IP address and tap \"Join\" on the ThirdPersonMobile app");
                EditorGUILayout.Space(10);
                DrawSectionDivider();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorPrefs.SetBool(PrefsShowGettingStarted, showFoldout);
        }

        private void DrawSectionConnection()
        {
            bool showConnectionSection = EditorPrefs.GetBool(PrefsShowConnection, true);
            showConnectionSection = EditorGUILayout.BeginFoldoutHeaderGroup(showConnectionSection, "Connection");

            if (showConnectionSection)
            {
                string serverPath = EditorPrefs.GetString(PrefsServerPath, "[not set]");
                EditorGUILayout.LabelField("You'll need a signalling server running on this PC. Download one here: https://github.com/bengreenier/node-dss");
                EditorGUILayout.Space(10);
                DrawLabelMessageButtonButton("Node DSS Server path:", serverPath, "Locate Folder", OnPressLocateNodeDssFolder, "Start Server", OnPressStartServer);
                DrawLabelMessageButton("This IP: (enter on mobile)", $"http://{ipAddress}:3000", "Refresh", () => ipAddress = GetLocalIPAddress());
                DrawRunInBackground();
                DrawSectionDivider();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorPrefs.SetBool(PrefsShowConnection, showConnectionSection);
        }

        private void DrawRunInBackground()
        {
            EditorGUILayout.LabelField("Run Application in Background", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Enable this setting to produce a smoother video stream for the mobile app");
            EditorGUILayout.LabelField("This affects the Player Settings for this project so revert before building");
            PlayerSettings.runInBackground = DrawLabelCheckbox("Run in background", PlayerSettings.runInBackground);
            EditorGUILayout.Space(10);
        }

        private void DrawSectionCameraFeeds()
        {
            EditorGUILayout.LabelField("Camera Quality", EditorStyles.boldLabel);
            bool streamResChanged = DrawResolutionDropdown("Streaming Camera Quality", ref mobileCamera.StreamResolution, "Resolution of the image hologram image streamed to the mobile. Also sets recording res. Reduced quality may improve performance/latency");
            if (streamResChanged)
            {
                mobileCamera.ResetStreamRenderTexture();
            }
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Cameras", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Below are the camera images that will be recorded, in to two separate files");
            GUILayout.BeginHorizontal();
            DrawCamTextureGroup("Scene Camera (used for recording)", mobileCamera.StreamCamera.targetTexture);
            GUI.enabled = Application.isPlaying;
            DrawCamTextureGroup("Mobile Camera Feed", textureReceiver.mostRecentTexture, "Update Preview Image", OnPressUpdatePreviewImage);
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }

        private void DrawSectionRecording()
        {
            EditorGUILayout.LabelField("Recording", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("When you hit record the files will end up in two different locations:");
            EditorGUILayout.LabelField("1. Scene Camera (left image) - will be saved on this PC");
            EditorGUILayout.LabelField("2. Mobile Camera (right image) - will be saved on the mobile phone");
            EditorGUILayout.Space(10);

            recordingFilename = EditorGUILayout.TextField("Filename", EditorPrefs.GetString(PrefsFilename, "MRMobileSceneCamera"));
            EditorPrefs.SetString(PrefsFilename, recordingFilename);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Recordings will be saved to..", EditorStyles.boldLabel);
            DrawLabelMessageButton("Scene Camera", GetLocalRecordingPath() + ".mp4", "Open Folder", OnPressOpenHologramFolder);
            DrawLabelMessageButton("Mobile Camera", GetMobileRecordingPath());

            GUILayout.BeginHorizontal();

            GUI.enabled = Application.isPlaying;
            if (GUILayout.Button(isRecording ? "Stop Recording" : "Start Recording"))
            {
                if (isRecording)
                {
                    OnPressStopRecording();
                }
                else
                {
                    OnPressStartRecording();
                }
            }
            GUI.enabled = true;

            GUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
        }

        private static void DrawSectionDivider()
        {
            EditorGUILayout.Space(20);
            Rect rect = EditorGUILayout.GetControlRect(true, 1);
            EditorGUI.DrawRect(rect, new Color(1f, 1f, 1f, 0.3f));
            EditorGUILayout.Space(10);
        }

        private void DrawCamTextureGroup(string label, Texture texture, string buttonLabel = null, Action onButtonPress = null)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(position.width / 2f - 20f));
            EditorGUILayout.LabelField(label);

            if (texture == null)
            {
                texture = Texture2D.blackTexture;
            }
            GUILayout.Box(texture, GUILayout.Width(position.width / 2f - 20f), GUILayout.Height(MaxWidthCameraImage / (16f / 9f)));

            if (!string.IsNullOrEmpty(buttonLabel))
            {
                if (GUILayout.Button(buttonLabel))
                {
                    onButtonPress?.Invoke();
                }
            }
            EditorGUILayout.EndVertical();
        }

        private static void DrawLabelMessage(string label, string message)
        {
            DrawLabelMessageButton(label, message);
        }

        /// <summary>
        /// Draws a GUI label, text message and button laid in a horizontal group 
        /// </summary>
        /// <param name="label"></param>
        /// <param name="message"></param>
        /// <param name="buttonText"></param>
        /// <param name="onButtonPress"></param>
        private static void DrawLabelMessageButton(string label, string message, string buttonText = null, Action onButtonPress = null)
        {
            DrawLabelMessageButtonButton(label, message, buttonText, onButtonPress);
        }

        private static void DrawLabelMessageButtonButton(string label, string message, string buttonText1 = null, Action onButtonPress1 = null, string buttonText2 = null, Action onButtonPress2 = null)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, message, GUILayout.Width(800));
            if (!string.IsNullOrEmpty(buttonText1))
            {
                if (GUILayout.Button(buttonText1, GUILayout.Width(130)))
                {
                    onButtonPress1?.Invoke();
                }
            }
            if (!string.IsNullOrEmpty(buttonText2))
            {
                if (GUILayout.Button(buttonText2, GUILayout.Width(130)))
                {
                    onButtonPress2?.Invoke();
                }
            }
            GUILayout.EndHorizontal();
        }

        private static int DrawLabelDropdown(string label, int selectedIndex, string[] options, string message)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            int index = EditorGUILayout.Popup(selectedIndex, options, GUILayout.Width(150));

            if (!string.IsNullOrEmpty(message))
            {
                EditorGUILayout.LabelField(message, GUILayout.Width(800));
            }

            EditorGUILayout.EndHorizontal();
            return index;
        }

        private static bool DrawResolutionDropdown(string label, ref CameraResolution.Size size, string message)
        {
            int currentIndex = (int)size;
            int newIndex = DrawLabelDropdown(label, currentIndex, CameraResolution.Labels, message);
            size = (CameraResolution.Size)newIndex;

            return currentIndex != newIndex;
        }

        private static bool DrawLabelCheckbox(string label, bool value)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            value = EditorGUILayout.Toggle(value);
            GUILayout.EndHorizontal();
            return value;
        }

        private static T FindObjectOfTypeWithError<T>() where T : UnityEngine.Object
        {
            T obj = FindObjectOfType<T>();
            if (obj == null)
            {
                Debug.LogWarning($"[MRMobileRemoting]: Could not find object in current scene with type: {typeof(T)}. Try installing components first");
            }
            return obj;
        }

        private static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new System.Exception("[MRMobileRemoting]: No network adapters with an IPv4 address in the system!");
        }

        private string GetLocalRecordingPath()
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "../Recordings/", recordingFilename));
        }

        private String GetMobileRecordingPath()
        {
            return "[Phone Storage]/Android/data/com.Microsoft.ThirdPersonMobile.Unity/CameraCapture";
        }

        private void OnPressLocateNodeDssFolder()
        {
            string serverPath = EditorUtility.OpenFolderPanel("Locate node-dss", "", "");
            EditorPrefs.SetString(PrefsServerPath, serverPath);
        }

        private void OnPressStartServer()
        {
            string serverPath = EditorPrefs.GetString(PrefsServerPath, null);
            if (string.IsNullOrEmpty(serverPath))
            {
                Debug.LogError("Locate node-dss folder first!");
            }

            Environment.SetEnvironmentVariable("DEBUG", "dss*");

            if (!Directory.Exists(Path.Combine(serverPath, "node_modules")))
            {
                // Have not yet run npm install
                using (Process process = new Process())
                {
                    process.StartInfo.WorkingDirectory = serverPath;
                    process.StartInfo.FileName = "npm";
                    process.StartInfo.Arguments = "install";
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    process.Start();
                    process.WaitForExit();
                }
            }

            using (Process process = new Process())
            {
                process.StartInfo.WorkingDirectory = serverPath;
                process.StartInfo.FileName = "npm";
                process.StartInfo.Arguments = "start";
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                process.Start();
            }
        }

        private void OnPressUpdatePreviewImage()
        {
            bool messageDidSend = messageSourceAndReceiver.SendRefreshCamera();
            if (!messageDidSend)
            {
                EditorUtility.DisplayDialog("Refresh Snapshot Error", "Command did not send - is the mobile companion app connected?", "OK");
            }
        }

        private void OnPressOpenHologramFolder()
        {
            EditorUtility.RevealInFinder(Path.GetDirectoryName(GetLocalRecordingPath()));
        }

        private void OnPressStartRecording()
        {
            bool messageDidSend = messageSourceAndReceiver.Send(Message.StartRecording);
            if (messageDidSend)
            {
                isRecording = true;
                Debug.Log("mobileCamera", mobileCamera);
                Debug.Log("mobileCamera.RecordingCamera", mobileCamera.StreamCamera);
                Debug.Log("mobileCamera.pixelWidth " + mobileCamera.StreamCamera.pixelWidth);

                if (gameViewRecorder == null)
                {
                    gameViewRecorder = new GameViewRecorder();
                }
                gameViewRecorder.StartRecording(GetLocalRecordingPath(),
                    mobileCamera.StreamCamera.tag,
                    mobileCamera.StreamCamera.pixelWidth,
                    mobileCamera.StreamCamera.pixelHeight,
                    true);
            }
            else
            {
                EditorUtility.DisplayDialog("Start Recording Error", "Could not start recording - is the mobile companion app connected?", "OK");
            }
        }

        private void OnPressStopRecording()
        {
            isRecording = false;
            messageSourceAndReceiver.Send(Message.StopRecording);
            gameViewRecorder.StopRecording();

            string message = @$"Rendered video saved on this PC:
{GetLocalRecordingPath() + ".mp4"}

Mobile Camera video saved on the mobile: 
{GetMobileRecordingPath()}";

            EditorUtility.DisplayDialog("Recordings Saved", message, "OK");
        }
    }
}