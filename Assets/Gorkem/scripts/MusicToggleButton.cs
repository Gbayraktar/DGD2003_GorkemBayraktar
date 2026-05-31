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

    private Image _image;
    private bool _isHovering;

    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    private void Start()
    {
        if (MainMenuAudio.Instance != null)
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
        if (MainMenuAudio.Instance != null)
            MainMenuAudio.Instance.ToggleMusic();

        UpdateSprite();
    }

    public void UpdateSprite()
    {
        if (_image == null) return;

        bool musicOn = MainMenuAudio.Instance == null || MainMenuAudio.Instance.IsMusicOn;

        Sprite target = musicOn
            ? (_isHovering ? onHoverSprite : onNormalSprite)
            : (_isHovering ? offHoverSprite : offNormalSprite);

        if (target != null)
            _image.sprite = target;
    }
}
