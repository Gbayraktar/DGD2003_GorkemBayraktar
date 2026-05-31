using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI butonlarına ekle (opsiyonel). MainMenuAudio zaten tüm Button'lara otomatik bağlanıyor.
/// Button olmayan tıklanabilir objeler için kullan.
/// </summary>
[RequireComponent(typeof(Button))]
public class UIButtonClickSound : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        if (MainMenuAudio.Instance != null)
            MainMenuAudio.Instance.PlayButtonClick();
    }
}
