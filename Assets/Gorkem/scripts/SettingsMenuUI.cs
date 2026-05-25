using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ana menüdeki Settings paneline Slider / Toggle bağla.
/// Değerler PlayerPrefs ile kalıcı kaydedilir.
/// </summary>
public class SettingsMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Toggle musicToggle;

    void Start()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        RefreshFromSave();
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
        RefreshFromSave();
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void OnVolumeChanged(float value)
    {
        if (GameSaveManager.Instance == null) return;
        GameSaveManager.Instance.MasterVolume = value;
        AudioListener.volume = value;
    }

    public void OnSensitivityChanged(float value)
    {
        if (GameSaveManager.Instance == null) return;
        GameSaveManager.Instance.MouseSensitivity = value;
    }

    public void OnMusicToggleChanged(bool isOn)
    {
        if (GameSaveManager.Instance == null) return;
        GameSaveManager.Instance.MusicOn = isOn;
        AudioListener.pause = !isOn;
    }

    void RefreshFromSave()
    {
        if (GameSaveManager.Instance == null) return;

        if (volumeSlider != null)
            volumeSlider.value = GameSaveManager.Instance.MasterVolume;

        if (sensitivitySlider != null)
            sensitivitySlider.value = GameSaveManager.Instance.MouseSensitivity;

        if (musicToggle != null)
            musicToggle.isOn = GameSaveManager.Instance.MusicOn;

        AudioListener.volume = GameSaveManager.Instance.MasterVolume;
        AudioListener.pause = !GameSaveManager.Instance.MusicOn;
    }
}
