using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Tooltip("Start butonuna basıldığında yüklenecek sahne adı (Build Settings'e eklenmiş olmalı)")]
    public string gameSceneName = "GameScene";

    [Header("Credits")]
    [Tooltip("Credits butonuna basıldığında açılıp/kapanacak panel")]
    public GameObject creditsPanel;

    void Start()
    {
        if (creditsPanel != null)
            creditsPanel.SetActive(false);
    }

    public void StartGame()
    {
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogWarning("MainMenu: gameSceneName boş. Inspector'dan sahne adını gir.");
            return;
        }

        SceneManager.LoadScene(gameSceneName);
    }

    public void ToggleCredits()
    {
        if (creditsPanel == null)
        {
            Debug.LogWarning("MainMenu: creditsPanel atanmamış. Inspector'dan Credits panelini ata.");
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
