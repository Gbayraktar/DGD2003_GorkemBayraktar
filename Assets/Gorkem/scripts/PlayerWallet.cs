using UnityEngine;
using System;

public class PlayerWallet : MonoBehaviour
{
    public static PlayerWallet Instance { get; private set; }

    [Header("Başlangıç Parası")]
    [SerializeField] private int startingMoney = 0;

    public int Money { get; private set; }

    // Para değişince UI'ı haberdar etmek için event
    public event Action<int> OnMoneyChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Money    = startingMoney;
    }

    public void AddMoney(int amount)
    {
        Money += amount;
        OnMoneyChanged?.Invoke(Money);
        Debug.Log($"Satış yapıldı! +{amount} para | Toplam: {Money}");
    }

    public bool SpendMoney(int amount)
    {
        if (Money < amount) return false;

        Money -= amount;
        OnMoneyChanged?.Invoke(Money);
        return true;
    }
}
