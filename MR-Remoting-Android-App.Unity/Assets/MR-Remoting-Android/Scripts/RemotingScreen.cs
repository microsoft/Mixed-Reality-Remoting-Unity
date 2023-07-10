using System.Collections.Generic;
using UnityEngine;

namespace MobileHolographicRemoting
{
    public class RemotingScreen : MonoBehaviour
    {
        [Header("Views")]
        [SerializeField] private GameObject IdleView;
        [SerializeField] private GameObject SettingsView;
        [SerializeField] private GameObject QRCodeView;

        [Header("Web RTC")]
        [SerializeField] private MessageSourceAndReceiver messageSourceAndReceiver;

        private Stack<GameObject> viewStack = new Stack<GameObject>();

        private void OnEnable()
        {
            IdleView.SetActive(false);
            QRCodeView.SetActive(false);
            SettingsView.SetActive(false);

            PushView(IdleView);
            PushView(SettingsView);
        }

        /// <summary>
        /// Push a view on to the view stack
        /// </summary>
        /// <param name="view"></param>
        private void PushView(GameObject view)
        {
            // Disable other views
            foreach (GameObject go in viewStack)
            {
                go.SetActive(false);
            }

            viewStack.Push(view);
            view.SetActive(true);
        }

        private void PopView()
        {
            if (viewStack.Count == 0)
            {
                Debug.LogError("View stack count is 0!");
                return;
            }

            viewStack.Pop().SetActive(false);

            if (viewStack.Count > 0)
            {
                viewStack.Peek().SetActive(true);
            }
        }

        #region Unity events

        public void OnClickOpen()
        {
            PushView(SettingsView);
        }

        public void OnClickClose()
        {
            PopView();
        }

        public void OnClickScanQR()
        {
            PushView(QRCodeView);
            messageSourceAndReceiver.Send(Message.QrCodeUiShownOnMobile);
        }

        public void OnClickQrBack()
        {
            PopView();
            messageSourceAndReceiver.Send(Message.QrCodeUiHiddenOnMobile);
        }

        public void OnHoloLensScannedQR()
        {
            if (!QRCodeView.activeInHierarchy)
            {
                Debug.LogError("RemotingScreen: received OnHoloLensScannedQR event. Expected QRCodeView to be active, but it's not!");
                return;
            }
            // PopView();
        }

        #endregion
    }
}