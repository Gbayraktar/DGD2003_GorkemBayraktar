using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class ButtonHoverColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Renkler")]
    public Color normalColor = Color.white;
    public Color hoverColor = Color.red;

    [Header("Hedef yazı (boş bırakılırsa otomatik aranır)")]
    public TMP_Text tmpText;
    public Text uiText;

    void Awake()
    {
        if (tmpText == null)
            tmpText = GetComponentInChildren<TMP_Text>(true);

        if (uiText == null)
            uiText = GetComponentInChildren<Text>(true);

        ApplyColor(normalColor);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ApplyColor(hoverColor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ApplyColor(normalColor);
    }

    void ApplyColor(Color c)
    {
        if (tmpText != null) tmpText.color = c;
        if (uiText != null) uiText.color = c;
    }
}
