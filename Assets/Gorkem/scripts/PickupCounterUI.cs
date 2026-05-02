using UnityEngine;
using TMPro;

public class PickupCounterUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI countText;

    [Header("Ayarlar")]
    [Tooltip("{0} yerine güncel obje sayısı yazılır")]
    [SerializeField] private string format = "Objeler: {0}";

    [Tooltip("Açıksa sadece IsSellable olanlar sayılır")]
    [SerializeField] private bool onlySellable = false;

    private void OnEnable()
    {
        if (onlySellable)
        {
            PickupObject.OnActiveSellableCountChanged += UpdateText;
            UpdateText(PickupObject.ActiveSellableCount);
        }
        else
        {
            PickupObject.OnActiveCountChanged += UpdateText;
            UpdateText(PickupObject.ActiveCount);
        }
    }

    private void OnDisable()
    {
        PickupObject.OnActiveCountChanged         -= UpdateText;
        PickupObject.OnActiveSellableCountChanged -= UpdateText;
    }

    private void UpdateText(int count)
    {
        if (countText == null) return;
        countText.text = string.Format(format, count);
    }
}
