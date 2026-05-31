using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Ayarları")]
    [Tooltip("Start butonuna basıldığında yüklenecek sahne adı (Build Settings'e eklenmiş olmalı)")]
    [SerializeField] public string gameSceneName = "GameScene";

    [Tooltip("Eğer sahne adı bulunamazsa bu Build Index kullanılacak (-1 = kullanma)")]
    [SerializeField] private int fallbackSceneBuildIndex = -1;

    [Header("Paneller")]
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Settings UI")]
    [SerializeField] private Slider volumeSlider;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;
    }

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;

        TryAutoAssignPanels();
        EnsureSaveManager();
        BindVolumeSlider();

        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        ApplySavedVolume();
    }

    public void StartGame()
    {
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError("[MainMenu] gameSceneName BOŞ. Inspector'dan sahne adını gir (örn: MainScene).", this);
            TryLoadFallback();
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            Debug.LogError($"[MainMenu] '{gameSceneName}' adlı sahne bulunamadı veya Build Settings içine eklenmemiş.", this);
            TryLoadFallback();
            return;
        }

        SceneManager.LoadScene(gameSceneName);
    }

    private void TryLoadFallback()
    {
        if (fallbackSceneBuildIndex >= 0 && fallbackSceneBuildIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(fallbackSceneBuildIndex);
    }

    public void OpenSettings()
    {
        if (settingsPanel == null)
        {
            Debug.LogWarning("[MainMenu] settingsPanel atanmamış.", this);
            return;
        }

        if (creditsPanel != null) creditsPanel.SetActive(false);
        settingsPanel.SetActive(true);
        RefreshVolumeSlider();
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void ToggleCredits()
    {
        if (creditsPanel == null)
        {
            Debug.LogWarning("[MainMenu] creditsPanel atanmamış.", this);
            return;
        }

        if (settingsPanel != null) settingsPanel.SetActive(false);
        creditsPanel.SetActive(!creditsPanel.activeSelf);
    }

    public void OnVolumeChanged(float value)
    {
        float v = Mathf.Clamp01(value);
        EnsureSaveManager();

        if (GameSaveManager.Instance != null)
            GameSaveManager.Instance.SfxVolume = v;

        if (MainMenuAudio.Instance != null)
            MainMenuAudio.Instance.SetSfxVolume(v);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void TryAutoAssignPanels()
    {
        if (settingsPanel == null)
        {
            GameObject found = GameObject.Find("SettingsPanel");
            if (found != null) settingsPanel = found;
        }

        if (volumeSlider == null && settingsPanel != null)
            volumeSlider = settingsPanel.GetComponentInChildren<Slider>(true);
    }

    private void BindVolumeSlider()
    {
        if (volumeSlider == null) return;

        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;
        volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    private void RefreshVolumeSlider()
    {
        if (volumeSlider == null || GameSaveManager.Instance == null) return;
        volumeSlider.SetValueWithoutNotify(GameSaveManager.Instance.SfxVolume);
    }

    private void ApplySavedVolume()
    {
        EnsureSaveManager();
        if (GameSaveManager.Instance == null) return;

        if (MainMenuAudio.Instance != null)
            MainMenuAudio.Instance.SetSfxVolume(GameSaveManager.Instance.SfxVolume);

        RefreshVolumeSlider();
    }

    private static void EnsureSaveManager()
    {
        if (GameSaveManager.Instance != null) return;
        new GameObject("GameSaveManager (Auto)").AddComponent<GameSaveManager>();
    }
}
