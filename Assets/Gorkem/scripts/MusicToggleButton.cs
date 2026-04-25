using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class MusicToggleButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Music ON görselleri")]
    public Sprite onNormalSprite;
    public Sprite onHoverSprite;

    [Header("Music OFF görselleri")]
    public Sprite offNormalSprite;
    public Sprite offHoverSprite;

    [Header("Başlangıç durumu")]
    [Tooltip("Açık mı kapalı mı başlasın")]
    public bool isMusicOn = true;

    private Image _image;
    private bool _isHovering;

    void Awake()
    {
        _image = GetComponent<Image>();
        UpdateSprite();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovering = true;
        UpdateSprite();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovering = false;
        UpdateSprite();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        isMusicOn = !isMusicOn;
        UpdateSprite();

        // İleride müzik aç/kapa kodu buraya gelecek:
        // AudioListener.pause = !isMusicOn;
        // veya kendi AudioManager'ına bağla.
    }

    void UpdateSprite()
    {
        if (_image == null) return;

        Sprite target;
        if (isMusicOn)
            target = _isHovering ? onHoverSprite : onNormalSprite;
        else
            target = _isHovering ? offHoverSprite : offNormalSprite;

        if (target != null)
            _image.sprite = target;
    }
}
