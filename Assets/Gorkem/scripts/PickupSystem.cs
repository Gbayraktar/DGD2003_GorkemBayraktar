using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PickupSystem : MonoBehaviour
{
    [Header("Ayarlar")]
    [SerializeField] private float pickupRange  = 2.5f;
    [SerializeField] private float throwForce   = 5f;
    [Tooltip("Sadece bu Layer'lardaki objeler raycast ile algılanır. 'Everything' bırakılabilir.")]
    [SerializeField] private LayerMask pickupMask = ~0;

    [Header("Tutma Noktası")]
    [SerializeField] private Transform holdPoint;

    [Header("Bırakma / Fırlatma")]
    [Tooltip("Yere bakarak bırakırken zemini bulmak için kullanılan raycast mesafesi.")]
    [SerializeField] private float dropRayDistance = 4f;
    [Tooltip("Obje zemine yerleştirilirken yüzeyden ne kadar yukarıda dursun.")]
    [SerializeField] private float surfaceOffset = 0.08f;

    [Header("Görsel Geri Bildirim")]
    [Tooltip("Bakılan objede gösterilecek prompt paneli (örn. 'E - Al')")]
    [SerializeField] private GameObject pickupPromptUI;
    [Tooltip("Opsiyonel — bakılan objenin ismini yazar")]
    [SerializeField] private TextMeshProUGUI itemNameText;
    [Tooltip("Opsiyonel — bakılan objenin fiyatını yazar")]
    [SerializeField] private TextMeshProUGUI itemPriceText;

    private PickupObject _heldObject;
    private PickupObject _lookingAt;
    private PickupObject _lastPromptTarget;
    private bool         _promptVisible;
    private Camera       _camera;

    private void Start()
    {
        _camera = Camera.main;

        if (_camera == null)
        {
            Debug.LogError("PickupSystem: Main Camera bulunamadı! Camera objesinin Tag'ini 'MainCamera' olarak ayarla.");
            _camera = FindFirstObjectByType<Camera>();
        }

        SetPromptVisible(false);
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

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, pickupMask, QueryTriggerInteraction.Ignore))
        {
            PickupObject pickable = hit.collider.GetComponentInParent<PickupObject>();

            if (pickable != null && !pickable.IsHeld)
                _lookingAt = pickable;
        }

        UpdatePromptUI();
    }

    private void UpdatePromptUI()
    {
        bool shouldShow = _lookingAt != null && _heldObject == null;

        if (shouldShow)
        {
            SetPromptVisible(true);

            if (_lookingAt != _lastPromptTarget)
            {
                if (itemNameText != null)
                    itemNameText.text = _lookingAt.ItemName;

                if (itemPriceText != null)
                    itemPriceText.text = _lookingAt.IsSellable ? $"${_lookingAt.Price}" : string.Empty;

                _lastPromptTarget = _lookingAt;
            }
        }
        else
        {
            SetPromptVisible(false);
            _lastPromptTarget = null;
        }
    }

    private void SetPromptVisible(bool visible)
    {
        if (pickupPromptUI == null) return;
        if (_promptVisible == visible) return;

        pickupPromptUI.SetActive(visible);
        _promptVisible = visible;
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

        SetPromptVisible(false);
        _lastPromptTarget = null;
    }

    private void DropObject()
    {
        _heldObject.Drop(GetDropPosition());
        _heldObject = null;
    }

    private void ThrowObject()
    {
        Vector3 throwDir = _camera.transform.forward * throwForce;
        _heldObject.Drop(Vector3.zero, throwDir);
        _heldObject = null;
    }

    private Vector3 GetDropPosition()
    {
        Ray ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));

        if (Physics.Raycast(ray, out RaycastHit hit, dropRayDistance, pickupMask, QueryTriggerInteraction.Ignore))
            return hit.point + hit.normal * surfaceOffset;

        Vector3 fallback = _camera.transform.position + _camera.transform.forward * 1.5f;

        if (Physics.Raycast(fallback + Vector3.up * 2f, Vector3.down, out RaycastHit groundHit, 5f, pickupMask, QueryTriggerInteraction.Ignore))
            fallback.y = groundHit.point.y + surfaceOffset;

        return fallback;
    }

    private void OnDrawGizmosSelected()
    {
        if (_camera == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(_camera.transform.position, _camera.transform.forward * pickupRange);
    }
}
