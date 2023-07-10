using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class JoystickControl : MonoBehaviour, IDragHandler, IEndDragHandler
{
    [SerializeField, Tooltip("Radius of this joystick (how far it can reach)")]
    private float radius = 70f;

    [SerializeField]
    private float sensitivity = 0.0001f;
 
    [Header("Events")]
    [SerializeField]
    private JoystickMovedEvent onJoystickMoved;

    public Vector2 Offset { get; private set; }
    private RectTransform rt;
    private Vector2 initialPosition;
    private Vector2 offsetFromInitial;
    private bool isDragging;

    private void Start()
    {
        rt = GetComponent<RectTransform>();
        initialPosition = rt.anchoredPosition;
        offsetFromInitial = initialPosition;
    }

    private void Update()
    {
        // Normalise our vector based on the joystick's radius
        Vector2 scaledOffset = offsetFromInitial / radius;
        Offset += scaledOffset * sensitivity;

        if (isDragging)
        {
            onJoystickMoved?.Invoke(Offset);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        isDragging = true;

        Vector2 targetPos = rt.anchoredPosition + eventData.delta;
        offsetFromInitial = targetPos - initialPosition;

        if (offsetFromInitial.magnitude > radius)
        {
            targetPos = offsetFromInitial.normalized * radius;
        }

        rt.anchoredPosition = targetPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        rt.anchoredPosition = initialPosition;
        offsetFromInitial = initialPosition;
    }
}
