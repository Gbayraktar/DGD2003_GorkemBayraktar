using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// M tuşu — sahnedeki tüm satılabilir objeleri anında satar (test/hile).
/// </summary>
public class SellAllCheat : MonoBehaviour
{
    [SerializeField] private SellArea sellArea;

    private void Update()
    {
        if (Keyboard.current == null) return;
        if (!Keyboard.current.mKey.wasPressedThisFrame) return;

        if (sellArea == null)
            sellArea = FindFirstObjectByType<SellArea>();

        if (sellArea == null)
        {
            Debug.LogWarning("[SellAllCheat] SellArea bulunamadı.");
            return;
        }

        sellArea.CheatSellAll();
    }
}
