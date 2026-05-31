using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// ESC: oyun durur, ana pause paneli açılır.
/// Settings: ayar paneli. Geri: ana panele döner.
/// ESC (her iki panelde): tüm paneller kapanır, oyun devam eder.
/// Volume: oyun müziğinin ses seviyesi (GameMusicPlayer + MasterVolume).
/// Slider'lar kodda otomatik bağlanır — Inspector OnValueChanged şart değil.
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance { get; private set; }
    public static bool IsOpen => Instance != null && Instance._isPaused;

    [Header("Paneller")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Ayarlar UI")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider sensitivitySlider;

    [Header("Hassasiyet (slider 0-1 → bu aralık)")]
    [SerializeField] private float fpsSensitivityMin = 0.05f;
    [SerializeField] private float fpsSensitivityMax = 0.5f;
    [SerializeField] private float cameraSensitivityMin = 0.05f;
    [SerializeField] private float cameraSensitivityMax = 0.5f;

    [Header("Quit")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Opsiyonel — otomatik bulunur")]
    [SerializeField] private FirstPersonCharacterController firstPersonController;
    [SerializeField] private ThirdPersonCamera thirdPersonCamera;

    private bool _isPaused;
    private bool _slidersBound;

    private void Awake()
    {
        Instance = this;

        TryAutoAssignPanels();
        TryAutoAssignSliders();

        if (pausePanel == gameObject)
            Debug.LogError("[PauseMenu] Script'i panelin ÜZERİNE koyma! Canvas veya 'GameUI' gibi her zaman aktif bir objeye taşı.", this);

        EnsureSaveManager();
        BindSliders();
        CloseAllPanelsImmediate();
        ApplySavedSettings();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (Keyboard.current == null) return;
        if (!Keyboard.current.escapeKey.wasPressedThisFrame) return;

        if (_isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    // --- Butonlar (UI OnClick) ---

    public void OnContinueClicked() => ResumeGame();

    public void OnSettingsClicked()
    {
        if (!_isPaused) PauseGame();

        if (pausePanel != null) SetPanelActive(pausePanel, false);
        if (settingsPanel != null) SetPanelActive(settingsPanel, true);

        RefreshPlayerReferences();
        BindSliders();
        ApplySavedSettings();
    }

    public void OnBackClicked()
    {
        if (settingsPanel != null) SetPanelActive(settingsPanel, false);
        if (pausePanel != null) SetPanelActive(pausePanel, true);
    }

    public void OnQuitClicked()
    {
        Time.timeScale = 1f;
        _isPaused = false;

        if (!string.IsNullOrEmpty(mainMenuSceneName) && Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
            SceneManager.LoadScene(mainMenuSceneName);
        else
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    /// <summary>Inspector'dan OnValueChanged bağlarsan bunu kullan.</summary>
    public void OnVolumeChanged(float value) => ApplyVolume(value);

    /// <summary>Inspector'dan OnValueChanged bağlarsan bunu kullan.</summary>
    public void OnSensitivityChanged(float value) => ApplySensitivitySetting(value);

    // --- Duraklat / devam ---

    private void PauseGame()
    {
        if (pausePanel == null)
        {
            Debug.LogError("[PauseMenu] Pause Panel atanmamış!", this);
            return;
        }

        _isPaused = true;
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (settingsPanel != null) SetPanelActive(settingsPanel, false);
        SetPanelActive(pausePanel, true);
    }

    private void ResumeGame()
    {
        _isPaused = false;
        Time.timeScale = 1f;

        CloseAllPanelsImmediate();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void CloseAllPanelsImmediate()
    {
        if (pausePanel != null) SetPanelActive(pausePanel, false);
        if (settingsPanel != null) SetPanelActive(settingsPanel, false);
    }

    // --- Slider kurulumu ---

    private void BindSliders()
    {
        if (_slidersBound && volumeSlider != null && sensitivitySlider != null)
            return;

        SetupSlider(volumeSlider, OnVolumeSliderChanged);
        SetupSlider(sensitivitySlider, OnSensitivitySliderChanged);

        _slidersBound = volumeSlider != null || sensitivitySlider != null;

        if (volumeSlider == null)
            Debug.LogWarning("[PauseMenu] Volume Slider bulunamadı. Settings panelindeki slider'ı Inspector'a sürükle.", this);

        if (sensitivitySlider == null)
            Debug.LogWarning("[PauseMenu] Sensitivity Slider bulunamadı.", this);
    }

    private static void SetupSlider(Slider slider, UnityEngine.Events.UnityAction<float> callback)
    {
        if (slider == null) return;

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;

        slider.onValueChanged.RemoveListener(callback);
        slider.onValueChanged.AddListener(callback);
    }

    private void OnVolumeSliderChanged(float value) => ApplyVolume(value);

    private void OnSensitivitySliderChanged(float value) => ApplySensitivitySetting(value);

    private void ApplyVolume(float rawValue)
    {
        float v = Normalize01(rawValue, volumeSlider);
        EnsureSaveManager();

        if (GameSaveManager.Instance != null)
            GameSaveManager.Instance.MasterVolume = v;

        ApplyMasterVolume(v);
    }

    private void ApplySensitivitySetting(float rawValue)
    {
        float v = Normalize01(rawValue, sensitivitySlider);
        EnsureSaveManager();

        if (GameSaveManager.Instance != null)
            GameSaveManager.Instance.MouseSensitivity = v;

        RefreshPlayerReferences();
        ApplySensitivity(v);
    }

    private static float Normalize01(float value, Slider slider)
    {
        if (slider == null) return Mathf.Clamp01(value);

        float min = slider.minValue;
        float max = slider.maxValue;
        if (max <= min) return Mathf.Clamp01(value);

        return Mathf.Clamp01((value - min) / (max - min));
    }

    private void RefreshSliders()
    {
        EnsureSaveManager();
        if (GameSaveManager.Instance == null) return;

        float vol = GameSaveManager.Instance.MasterVolume;
        if (vol <= 0.001f) vol = 1f;

        if (volumeSlider != null)
            volumeSlider.SetValueWithoutNotify(vol);

        if (sensitivitySlider != null)
            sensitivitySlider.SetValueWithoutNotify(GameSaveManager.Instance.MouseSensitivity);
    }

    private void ApplySavedSettings()
    {
        EnsureSaveManager();
        if (GameSaveManager.Instance == null) return;

        float vol = GameSaveManager.Instance.MasterVolume;
        if (vol <= 0.001f)
            vol = 1f;

        ApplyMasterVolume(vol);
        RefreshPlayerReferences();
        ApplySensitivity(GameSaveManager.Instance.MouseSensitivity);
        RefreshSliders();
    }

    private void RefreshPlayerReferences()
    {
        if (firstPersonController == null)
            firstPersonController = FindFirstObjectByType<FirstPersonCharacterController>();

        if (thirdPersonCamera == null)
            thirdPersonCamera = FindFirstObjectByType<ThirdPersonCamera>();
    }

    private static void ApplyMasterVolume(float volume)
    {
        float v = Mathf.Clamp01(volume);

        if (GameMusicPlayer.Instance != null)
            GameMusicPlayer.Instance.SetVolumeMultiplier(v);
    }

    private void ApplySensitivity(float normalized01)
    {
        float t = Mathf.Clamp01(normalized01);

        if (firstPersonController != null)
            firstPersonController.SetMouseSensitivityScale(t, fpsSensitivityMin, fpsSensitivityMax);

        if (thirdPersonCamera != null)
            thirdPersonCamera.SetSensitivityScale(t, cameraSensitivityMin, cameraSensitivityMax);
    }

    private void TryAutoAssignSliders()
    {
        if (settingsPanel == null) return;

        Slider[] sliders = settingsPanel.GetComponentsInChildren<Slider>(true);

        if (volumeSlider == null)
        {
            foreach (Slider s in sliders)
            {
                string n = s.name.ToLowerInvariant();
                if (n.Contains("volume") || n.Contains("ses") || n.Contains("sound") || n.Contains("music"))
                {
                    volumeSlider = s;
                    break;
                }
            }
        }

        if (sensitivitySlider == null)
        {
            foreach (Slider s in sliders)
            {
                string n = s.name.ToLowerInvariant();
                if (n.Contains("sens") || n.Contains("mouse") || n.Contains("hassas"))
                {
                    sensitivitySlider = s;
                    break;
                }
            }
        }

        // İsim eşleşmezse: settings panelindeki ilk iki slider
        if (sliders.Length >= 1 && volumeSlider == null)
            volumeSlider = sliders[0];

        if (sliders.Length >= 2 && sensitivitySlider == null)
            sensitivitySlider = sliders[1];
    }

    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel == null) return;
        if (panel == gameObject)
        {
            Debug.LogWarning("[PauseMenu] Panel referansı script'in kendi objesi.", this);
            return;
        }

        if (active)
            SetParentsActive(panel.transform);

        panel.SetActive(active);
    }

    private void SetParentsActive(Transform t)
    {
        while (t != null)
        {
            if (!t.gameObject.activeSelf)
                t.gameObject.SetActive(true);
            t = t.parent;
        }
    }

    private void TryAutoAssignPanels()
    {
        if (pausePanel == null)
            pausePanel = FindInactiveByNames("PausePanel", "Pause Menu", "PAUSEMENU", "PauseMenu");

        if (settingsPanel == null)
            settingsPanel = FindInactiveByNames("SettingsPanel", "Settings Panel", "SettingsMenu", "Settings");
    }

    private static GameObject FindInactiveByNames(params string[] names)
    {
        foreach (string n in names)
        {
            GameObject found = GameObject.Find(n);
            if (found != null) return found;
        }

        Transform[] all = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (Transform t in all)
        {
            if (t == null || t.hideFlags != HideFlags.None) continue;
            if (t.gameObject.scene.name == null) continue;

            foreach (string n in names)
            {
                if (t.name.Equals(n, System.StringComparison.OrdinalIgnoreCase))
                    return t.gameObject;
            }
        }

        return null;
    }

    private static void EnsureSaveManager()
    {
        if (GameSaveManager.Instance != null) return;

        var go = new GameObject("GameSaveManager (Auto)");
        go.AddComponent<GameSaveManager>();
    }
}
