using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class SellLogUI : MonoBehaviour
{
    [Header("UI Referansları")]
    [SerializeField] private TextMeshProUGUI soldItemsText;
    [SerializeField] private TextMeshProUGUI totalMoneyText;
    [SerializeField] private TextMeshProUGUI collectableValueText;

    [Header("Ayarlar")]
    [SerializeField] private int maxVisibleItems = 10;

    private readonly List<string> _soldEntries = new();
    private int _totalEarned;
    private int _remainingCount;

    private void OnEnable()
    {
        SellArea.OnItemSold += HandleItemSold;

        if (PlayerWallet.Instance != null)
            _totalEarned = PlayerWallet.Instance.Money;
    }

    private void OnDisable()
    {
        SellArea.OnItemSold -= HandleItemSold;
    }

    private void Start()
    {
        _remainingCount = FindObjectsByType<PickupObject>(FindObjectsSortMode.None)
                          .Count(p => p.IsSellable);

        UpdateCollectableText();
    }

    private void HandleItemSold(string itemName, int price)
    {
        _totalEarned    += price;
        _remainingCount--;

        _soldEntries.Add($"Para: +{price}");

        while (_soldEntries.Count > maxVisibleItems)
            _soldEntries.RemoveAt(0);

        RefreshUI();
        UpdateCollectableText();
    }

    private void RefreshUI()
    {
        if (soldItemsText != null)
            soldItemsText.text = string.Join("\n", _soldEntries);

        if (totalMoneyText != null)
            totalMoneyText.text = $"Toplam: {_totalEarned}$";
    }

    private void UpdateCollectableText()
    {
        if (collectableValueText == null) return;

        collectableValueText.text = $"Bulunabilir objeler: {_remainingCount}";
    }
}
