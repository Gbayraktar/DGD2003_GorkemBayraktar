using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PickupObject : MonoBehaviour
{
    [Header("Ayarlar")]
    [SerializeField] private string itemName            = "Obje";
    [SerializeField] private int    price               = 10;
    [SerializeField] private bool   isSellable          = true;
    [SerializeField] private float  heldScaleMultiplier = 0.4f;
    [SerializeField] private float  followSpeed         = 20f;

    [Header("Başlangıç Fizik")]
    [Tooltip("Açıksa obje havada asılı başlar, ilk alınıp bırakıldıktan sonra fizik devreye girer")]
    [SerializeField] private bool startFloating = false;

    private Rigidbody _rb;
    private Collider  _col;
    private Transform _holdPoint;
    private Vector3   _originalScale;

    public bool   IsHeld    { get; private set; }
    public bool   IsSellable => isSellable;
    public string ItemName  => itemName;
    public int    Price     => price;

    private void Awake()
    {
        _rb            = GetComponent<Rigidbody>();
        _col           = GetComponent<Collider>();
        _originalScale = transform.localScale;

        if (startFloating)
        {
            _rb.isKinematic = true;
            _rb.useGravity  = false;
        }
    }

    private void FixedUpdate()
    {
        if (!IsHeld || _holdPoint == null) return;

        // Rigidbody ile smooth takip — parenting yok, scale bozulmuyor
        _rb.linearVelocity = Vector3.zero;
        _rb.MovePosition(Vector3.Lerp(transform.position, _holdPoint.position, followSpeed * Time.fixedDeltaTime));
        _rb.MoveRotation(Quaternion.Slerp(transform.rotation, _holdPoint.rotation, followSpeed * Time.fixedDeltaTime));
    }

    public void Pickup(Transform holdPoint)
    {
        IsHeld     = true;
        _holdPoint = holdPoint;

        _rb.isKinematic     = true;
        _rb.useGravity      = false;
        _rb.linearVelocity  = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        // Elde tutulurken collider'ı kapat — oyuncuyu itmesin
        _col.enabled = false;

        transform.localScale = _originalScale * heldScaleMultiplier;
    }

    public void Drop(Vector3 dropPosition, Vector3 throwForce = default)
    {
        IsHeld     = false;
        _holdPoint = null;

        // Objeyi oyuncunun önüne taşı, sonra fiziği ve collider'ı aç
        transform.position   = dropPosition;
        transform.localScale = _originalScale;

        _col.enabled    = true;
        _rb.isKinematic = false;
        _rb.useGravity  = true;

        if (throwForce != Vector3.zero)
            _rb.AddForce(throwForce, ForceMode.Impulse);
    }
}
