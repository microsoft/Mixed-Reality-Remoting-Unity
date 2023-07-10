using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MobileHolographicRemoting
{
    /// <summary>
    /// Periodic flashing of the recording icon
    /// </summary>
    public class RecordingIcon : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField, Range(1f, 10f)] private float speed = 3f;

        private void Update()
        {
            float t = Mathf.Cos(Time.time * speed);
            bool on = t > 0;
            iconImage.enabled = on;
        }
    }
}