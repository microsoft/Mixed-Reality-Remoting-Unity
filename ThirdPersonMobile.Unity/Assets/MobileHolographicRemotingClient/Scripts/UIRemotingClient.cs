using Microsoft.MixedReality.WebRTC.Unity;
using TMPro;
using UnityEngine;

/// <summary>
/// Links input form with the connection/signaler
/// 
/// Hides UI once connected
/// </summary>
public class UIRemotingClient : MonoBehaviour
{
    private const string PrefsAddressKey = "HolographicRemotingAddressKey";
    private const string DefaultAddress = "127.0.0.1";

    [Header("WebRTC components")]
    [SerializeField] private NodeDssSignaler nodeDssSignaler;
    [SerializeField] private PeerConnection peerConnection;

    [Header("UI Components")]
    [SerializeField] private TMP_InputField ipAddressInput;
    [SerializeField] private TMP_Text ipAddressInputDescription;
    [SerializeField] private GameObject joinScreen;
    [SerializeField] private GameObject remotingScreen;

    private bool didConnect;

    private void Start()
    {
        joinScreen.SetActive(true);
        remotingScreen.SetActive(false);

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        peerConnection.OnInitialized.AddListener(PeerConnection_OnInitialized);

        ipAddressInput.text = PlayerPrefs.GetString(PrefsAddressKey, DefaultAddress);
        ipAddressInput.onValueChanged.AddListener(value => ipAddressInputDescription.text = GetFullIpAddress(ipAddressInput.text));
    }

    private void Update()
    {
        if (didConnect)
        {
            joinScreen.SetActive(false);
            remotingScreen.SetActive(true);

            didConnect = false;
        }
    }

    /// <summary>
    /// Example: for input 127.0.0.1, returns http://127.0.0.1:3000 
    /// </summary>
    /// <param name="ipDecimals"></param>
    /// <returns></returns>
    private static string GetFullIpAddress(string ipDecimals)
    {
        return $"http://{ipDecimals}:3000/";
    }

    #region Unity UI events

    public void OnClickJoin()
    {
        nodeDssSignaler.HttpServerAddress = GetFullIpAddress(ipAddressInput.text);
        PlayerPrefs.SetString(PrefsAddressKey, ipAddressInput.text);
        peerConnection.StartConnection();
    }

    public void OnToggleRecord(bool value)
    {
        Debug.Log("Toggle record " + value);
    }

    #endregion

    #region PeerConnection events

    private void PeerConnection_OnInitialized()
    {
        Debug.Log("Peer connection init");
        peerConnection.Peer.Connected += Peer_OnConnected;
    }

    private void Peer_OnConnected()
    {
        Debug.Log("Peer " + peerConnection.Peer.IsConnected);
        didConnect = true;
    }

    #endregion
}
