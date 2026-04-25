using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Tooltip("Start butonuna basıldığında yüklenecek sahne adı (Build Settings'e eklenmiş olmalı)")]
    public string gameSceneName = "GameScene";

    public void StartGame()
    {
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogWarning("MainMenu: gameSceneName boş. Inspector'dan sahne adını gir.");
            return;
        }

        SceneManager.LoadScene(gameSceneName);
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
