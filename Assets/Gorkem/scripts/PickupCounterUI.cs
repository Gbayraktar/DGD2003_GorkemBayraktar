using UnityEngine;
using TMPro;

public class PickupCounterUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI countText;

    [Header("Ayarlar")]
    [Tooltip("{0} yerine güncel obje sayısı yazılır")]
    [SerializeField] private string format = "Objeler: {0}";

    [Tooltip("Açıksa sadece IsSellable olanlar sayılır (event yerine tarama kullanır)")]
    [SerializeField] private bool onlySellable = false;

    private void OnEnable()
    {
        PickupObject.OnActiveCountChanged += HandleCountChanged;
        RefreshNow();
    }

    private void OnDisable()
    {
        PickupObject.OnActiveCountChanged -= HandleCountChanged;
    }

    private void HandleCountChanged(int count)
    {
        if (onlySellable)
            RefreshNow();
        else
            UpdateText(count);
    }

    private void RefreshNow()
    {
        int count;

        if (onlySellable)
        {
            var all = FindObjectsByType<PickupObject>(FindObjectsSortMode.None);
            count = 0;
            for (int i = 0; i < all.Length; i++)
                if (all[i].IsSellable) count++;
        }
        else
        {
            count = PickupObject.ActiveCount;
        }

        UpdateText(count);
    }

    private void UpdateText(int count)
    {
        if (countText == null) return;
        countText.text = string.Format(format, count);
    }
}
