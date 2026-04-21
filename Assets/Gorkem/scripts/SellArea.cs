using UnityEngine;
using System;

public class SellArea : MonoBehaviour
{
    [Header("Görsel")]
    [SerializeField] private Color gizmoColor = new Color(0f, 1f, 0.3f, 0.35f);

    [Header("VFX")]
    [Tooltip("Obje satıldığında oynatılacak varsayılan efekt prefab'ı. PickupObject'e özel efekt atanmışsa o öncelikli olur.")]
    [SerializeField] private GameObject sellVfxPrefab;
    [Tooltip("Spawn edilen efektin otomatik yok edilme süresi (saniye).")]
    [SerializeField] private float vfxLifetime = 3f;
    [Tooltip("Efekti objenin pozisyonuna Y ekseninde kadar yükselt (yerde spawn olmasın diye).")]
    [SerializeField] private float vfxYOffset = 0.5f;

    [Header("SFX (opsiyonel)")]
    [SerializeField] private AudioClip sellSfx;
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    public static event Action<string, int> OnItemSold;

    private void OnTriggerEnter(Collider other)
    {
        PickupObject item = other.GetComponent<PickupObject>();

        if (item == null || item.IsHeld) return;

        Sell(item);
    }

    private void OnTriggerStay(Collider other)
    {
        PickupObject item = other.GetComponent<PickupObject>();

        if (item == null || item.IsHeld) return;

        Sell(item);
    }

    private void Sell(PickupObject item)
    {
        if (PlayerWallet.Instance == null)
        {
            Debug.LogWarning("SellArea: Sahnede PlayerWallet bulunamadı!");
            return;
        }

        PlayerWallet.Instance.AddMoney(item.Price);
        OnItemSold?.Invoke(item.ItemName, item.Price);
        Debug.Log($"{item.ItemName} satıldı! +{item.Price} para");

        SpawnSellEffects(item);

        Destroy(item.gameObject);
    }

    private void SpawnSellEffects(PickupObject item)
    {
        Vector3 spawnPos = item.transform.position + Vector3.up * vfxYOffset;

        GameObject vfx = item.CustomSellVfx != null ? item.CustomSellVfx : sellVfxPrefab;
        if (vfx != null)
        {
            GameObject spawned = Instantiate(vfx, spawnPos, item.transform.rotation);
            if (vfxLifetime > 0f) Destroy(spawned, vfxLifetime);
        }

        AudioClip clip = item.CustomSellSfx != null ? item.CustomSellSfx : sellSfx;
        if (clip != null)
            AudioSource.PlayClipAtPoint(clip, spawnPos, sfxVolume);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;

        MeshCollider mesh = GetComponent<MeshCollider>();
        BoxCollider  box  = GetComponent<BoxCollider>();

        if (box != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
        }
        else
        {
            Gizmos.DrawCube(transform.position, transform.localScale);
        }
    }
}
