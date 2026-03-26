using UnityEngine;

public class SellArea : MonoBehaviour
{
    [Header("Görsel")]
    [SerializeField] private Color gizmoColor = new Color(0f, 1f, 0.3f, 0.35f);

    private void OnTriggerEnter(Collider other)
    {
        PickupObject item = other.GetComponent<PickupObject>();

        // Elde tutulan objeler satılmasın, sadece bırakılanlar
        if (item == null || item.IsHeld) return;

        Sell(item);
    }

    // Trigger içinde duran obje bırakılınca da sat
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
        Debug.Log($"{item.ItemName} satıldı! +{item.Price} para");

        Destroy(item.gameObject);
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
