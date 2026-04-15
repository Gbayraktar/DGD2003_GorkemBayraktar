using UnityEngine;
using UnityEngine.Events;

public class OrderBubble : MonoBehaviour
{
    [Header("Etkileşim")]
    [SerializeField] private float interactionRange = 3f;

    [Header("Görsel Geri Bildirim")]
    [SerializeField] private GameObject outOfRangeIndicator;  // Çok uzaktayken gösterilecek obje (opsiyonel)
    [SerializeField] private Color      inRangeColor    = Color.green;
    [SerializeField] private Color      outOfRangeColor = Color.red;
    [SerializeField] private Renderer   bubbleRenderer;

    [Header("Olay")]
    public UnityEvent OnInteracted; // Inspector'dan fonksiyon bağlanabilir

    private Transform _player;
    private bool      _playerInRange = false;

    private void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) _player = playerObj.transform;

        if (outOfRangeIndicator != null)
            outOfRangeIndicator.SetActive(false);
    }

    private void Update()
    {
        if (_player == null) return;

        float distance = Vector3.Distance(transform.position, _player.position);
        _playerInRange = distance <= interactionRange;

        UpdateVisuals();
    }

    // Unity'nin built-in tıklama eventi — Collider gerekli
    private void OnMouseDown()
    {
        if (_player == null) return;

        if (!_playerInRange)
        {
            Debug.Log($"[OrderBubble] Çok uzakta! Önce yaklaş. (Mesafe: {Vector3.Distance(transform.position, _player.position):F1} / Gerekli: {interactionRange})");
            return;
        }

        Interact();
    }

    private void Interact()
    {
        Debug.Log($"[OrderBubble] '{gameObject.name}' ile etkileşime girildi!");
        OnInteracted?.Invoke();
    }

    private void UpdateVisuals()
    {
        // Bubble rengi değiştir
        if (bubbleRenderer != null)
            bubbleRenderer.material.color = _playerInRange ? inRangeColor : outOfRangeColor;

        // Uzakta olduğunu gösteren ikon
        if (outOfRangeIndicator != null)
            outOfRangeIndicator.SetActive(!_playerInRange);
    }

    // Dışarıdan mesafe kontrolü için kullanılabilir
    public bool IsPlayerInRange() => _playerInRange;

    // Editörde etkileşim menzilini göster
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawSphere(transform.position, interactionRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
