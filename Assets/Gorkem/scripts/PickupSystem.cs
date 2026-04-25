using UnityEngine;
using UnityEngine.InputSystem;

public class PickupSystem : MonoBehaviour
{
    [Header("Ayarlar")]
    [SerializeField] private float pickupRange  = 2.5f;
    [SerializeField] private float throwForce   = 5f;

    [Header("Tutma Noktası")]
    [SerializeField] private Transform holdPoint;

    [Header("Görsel Geri Bildirim")]
    [SerializeField] private GameObject pickupPromptUI;

    private PickupObject _heldObject;
    private PickupObject _lookingAt;
    private Camera _camera;

    private void Start()
    {
        _camera = Camera.main;

        if (_camera == null)
        {
            Debug.LogError("PickupSystem: Main Camera bulunamadı! Camera objesinin Tag'ini 'MainCamera' olarak ayarla.");
            _camera = FindFirstObjectByType<Camera>();
        }

        if (pickupPromptUI != null)
            pickupPromptUI.SetActive(false);
    }

    private void Update()
    {
        if (_camera == null) return;

        DetectObject();
        HandleInput();
    }

    private void DetectObject()
    {
        _lookingAt = null;

        Ray ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
        {
            PickupObject pickable = hit.collider.GetComponent<PickupObject>();

            if (pickable != null && !pickable.IsHeld)
                _lookingAt = pickable;
        }

        // UI prompt göster/gizle
        if (pickupPromptUI != null)
            pickupPromptUI.SetActive(_lookingAt != null && _heldObject == null);
    }

    private void HandleInput()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (_heldObject != null)
                DropObject();
            else if (_lookingAt != null)
                PickupObject();
        }

        // G tuşuyla fırlat
        if (Keyboard.current.gKey.wasPressedThisFrame && _heldObject != null)
            ThrowObject();
    }

    private void PickupObject()
    {
        if (holdPoint == null)
        {
            Debug.LogWarning("Hold Point atanmamış! Player objesine bir HoldPoint child objesi ekle.");
            return;
        }

        _heldObject = _lookingAt;
        _heldObject.Pickup(holdPoint);
    }

    private void DropObject()
    {
        // Objeyi oyuncunun önüne bırak, altına değil
        Vector3 dropPos = _camera.transform.position + _camera.transform.forward * 1.5f;
        _heldObject.Drop(dropPos);
        _heldObject = null;
    }

    private void ThrowObject()
    {
        Vector3 dropPos  = _camera.transform.position + _camera.transform.forward * 1.5f;
        Vector3 throwDir = _camera.transform.forward * throwForce;
        _heldObject.Drop(dropPos, throwDir);
        _heldObject = null;
    }

    // Raycast menzilini editörde göster
    private void OnDrawGizmosSelected()
    {
        if (_camera == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(_camera.transform.position, _camera.transform.forward * pickupRange);
    }
}
