using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Hedef")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.6f, 0f);

    [Header("Mesafe")]
    [SerializeField] private float distance = 4f;
    [SerializeField] private float minDistance = 1.5f;
    [SerializeField] private float maxDistance = 8f;

    [Header("Orbit")]
    [SerializeField] private float sensitivity = 0.2f;
    [SerializeField] private float verticalMin = -15f;
    [SerializeField] private float verticalMax = 60f;

    [Header("Yumuşatma")]
    [SerializeField] private float followSmoothing = 15f;

    [Header("Çarpışma")]
    [SerializeField] private LayerMask collisionMask = ~0;

    private float _yaw;
    private float _pitch = 20f;

    private void Start()
    {
        _yaw   = transform.eulerAngles.y;
        _pitch = transform.eulerAngles.x;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        HandleOrbit();
        HandleZoom();
        HandleCursorToggle();
    }

    private void HandleOrbit()
    {
        if (Mouse.current != null)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            _yaw   += delta.x * sensitivity;
            _pitch -= delta.y * sensitivity;
            _pitch  = Mathf.Clamp(_pitch, verticalMin, verticalMax);
        }

        Quaternion rotation    = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3    pivotPoint  = target.position + targetOffset;
        Vector3    desiredPos  = pivotPoint - rotation * Vector3.forward * distance;

        // Kamera engele çarpmasın diye kısa tut
        if (Physics.Linecast(pivotPoint, desiredPos, out RaycastHit hit, collisionMask))
            desiredPos = hit.point + hit.normal * 0.2f;

        transform.position = Vector3.Lerp(transform.position, desiredPos, followSmoothing * Time.deltaTime);
        transform.LookAt(pivotPoint);
    }

    private void HandleZoom()
    {
        if (Mouse.current == null) return;

        float scroll = Mouse.current.scroll.ReadValue().y;
        distance -= scroll * 0.5f;
        distance  = Mathf.Clamp(distance, minDistance, maxDistance);
    }

    private void HandleCursorToggle()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }
        else if (Mouse.current != null &&
                 Mouse.current.leftButton.wasPressedThisFrame &&
                 Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }
    }
}
