using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Ayarları")]
    [Tooltip("Start butonuna basıldığında yüklenecek sahne adı.\nÖRN: GameScene\nÖNEMLİ: Sahne, File > Build Settings içine eklenmiş olmalı!")]
    [SerializeField] public string gameSceneName = "GameScene";

    [Tooltip("Eğer sahne adı bulunamazsa bu Build Index kullanılacak (-1 = kullanma)")]
    [SerializeField] private int fallbackSceneBuildIndex = -1;

    [Header("Credits")]
    [Tooltip("Credits butonuna basıldığında açılıp/kapanacak panel")]
    public GameObject creditsPanel;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;
    }

    void OnEnable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;

        if (creditsPanel != null)
            creditsPanel.SetActive(false);
    }

    public void StartGame()
    {
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError("[MainMenu] gameSceneName BOŞ. Inspector'dan sahne adını gir (örn: GameScene).", this);
            TryLoadFallback();
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            Debug.LogError($"[MainMenu] '{gameSceneName}' adlı sahne bulunamadı veya Build Settings içine eklenmemiş. " +
                           "File > Build Settings... > Scenes In Build listesine ekleyip 'Add Open Scenes' yap.", this);
            TryLoadFallback();
            return;
        }

        Debug.Log($"[MainMenu] '{gameSceneName}' yükleniyor...", this);
        SceneManager.LoadScene(gameSceneName);
    }

    private void TryLoadFallback()
    {
        if (fallbackSceneBuildIndex >= 0 && fallbackSceneBuildIndex < SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log($"[MainMenu] Fallback olarak Build Index {fallbackSceneBuildIndex} yükleniyor...", this);
            SceneManager.LoadScene(fallbackSceneBuildIndex);
        }
    }

    public void ToggleCredits()
    {
        if (creditsPanel == null)
        {
            Debug.LogWarning("[MainMenu] creditsPanel atanmamış. Inspector'dan Credits panelini ata.", this);
            return;
        }

        creditsPanel.SetActive(!creditsPanel.activeSelf);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
