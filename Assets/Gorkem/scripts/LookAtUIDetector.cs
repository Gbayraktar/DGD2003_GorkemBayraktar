using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Ekranın merkezinden raycast atar ve karşısındaki <see cref="LookAtInfo"/>
/// objesi için bir UI paneli gösterir.
///
/// İki mod:
///  - <b>AlwaysWhileLooking</b>: Bakarken sürekli açık, bakmayınca kapalı.
///  - <b>OnKeyPress</b>: Bakarken E'ye basınca <see cref="showDuration"/> saniye açık kalıp otomatik kapanır.
/// </summary>
public class LookAtUIDetector : MonoBehaviour
{
    public enum DisplayMode
    {
        AlwaysWhileLooking,
        OnKeyPress,
    }

    [Header("Algılama")]
    [SerializeField] private float range = 3f;
    [SerializeField] private LayerMask mask = ~0;
    [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

    [Header("Davranış")]
    [SerializeField] private DisplayMode mode = DisplayMode.OnKeyPress;
    [Tooltip("OnKeyPress modunda paneli kaç saniye açık tutalım?")]
    [SerializeField] private float showDuration = 1f;
    [Tooltip("OnKeyPress modunda hangi tuşa basılacak.")]
    [SerializeField] private Key interactKey = Key.E;

    [Header("UI")]
    [Tooltip("Bakılınca / E'ye basılınca açılacak panel (başlangıçta kapalı olabilir).")]
    [SerializeField] private GameObject infoPanel;
    [Tooltip("Opsiyonel — atanırsa LookAtInfo.Message bu alana yazılır. Boş bırakılırsa UI'daki yazıya dokunulmaz.")]
    [SerializeField] private TextMeshProUGUI infoText;

    private Camera     _camera;
    private LookAtInfo _current;
    private bool       _panelVisible;
    private float      _hideAtTime = -1f;

    private void Start()
    {
        _camera = Camera.main;
        if (_camera == null)
            _camera = FindFirstObjectByType<Camera>();

        SetPanelVisible(false);
    }

    private void Update()
    {
        if (_camera == null) return;

        _current = DetectLookTarget();

        if (mode == DisplayMode.AlwaysWhileLooking)
            UpdateAlwaysMode();
        else
            UpdateKeyPressMode();
    }

    private LookAtInfo DetectLookTarget()
    {
        Ray ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));

        if (Physics.Raycast(ray, out RaycastHit hit, range, mask, triggerInteraction))
            return hit.collider.GetComponentInParent<LookAtInfo>();

        return null;
    }

    private void UpdateAlwaysMode()
    {
        if (_current != null)
            ShowMessage(_current.Message);
        else
            SetPanelVisible(false);
    }

    private void UpdateKeyPressMode()
    {
        bool keyPressed = Keyboard.current != null && Keyboard.current[interactKey].wasPressedThisFrame;

        if (keyPressed && _current != null)
        {
            ShowMessage(_current.Message);
            _hideAtTime = Time.time + showDuration;
        }

        if (_panelVisible && Time.time >= _hideAtTime)
            SetPanelVisible(false);
    }

    private void ShowMessage(string text)
    {
        if (infoText != null)
            infoText.text = text;

        SetPanelVisible(true);
    }

    private void SetPanelVisible(bool visible)
    {
        if (infoPanel == null) return;
        if (_panelVisible == visible) return;

        infoPanel.SetActive(visible);
        _panelVisible = visible;
    }

    private void OnDrawGizmosSelected()
    {
        Camera cam = _camera != null ? _camera : Camera.main;
        if (cam == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(cam.transform.position, cam.transform.forward * range);
    }
}
