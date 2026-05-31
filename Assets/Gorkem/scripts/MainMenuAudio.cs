using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ana menü müziği + buton tıklama sesi.
/// Müzik: ses butonu ile aç/kapa.
/// Slider: sadece buton tıklama sesinin seviyesini ayarlar.
/// </summary>
public class MainMenuAudio : MonoBehaviour
{
    public static MainMenuAudio Instance { get; private set; }

    [Header("Müzik — mp3/wav dosyasını BURAYA sürükle")]
    [SerializeField] private AudioClip musicClip;

    [Tooltip("Boş bırakılırsa otomatik AudioSource oluşturulur.")]
    [SerializeField] private AudioSource musicSource;

    [SerializeField] private float musicVolume = 0.5f;

    [Header("Buton Sesi — tıklama sesi dosyasını buraya sürükle")]
    [SerializeField] private AudioClip buttonClickClip;

    [Header("Otomatik")]
    [Tooltip("Sahnedeki tüm UI Button'lara tıklama sesi bağla.")]
    [SerializeField] private bool autoWireAllButtons = true;

    private AudioSource _sfxSource;

    public bool IsMusicOn { get; private set; } = true;
    public float SfxVolume { get; private set; } = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        SetupAudioSources();
        LoadSettings();
        ApplyMusicState();
    }

    private void Start()
    {
        if (autoWireAllButtons)
            WireAllButtons();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void SetupAudioSources()
    {
        if (musicSource == null)
            musicSource = GetComponent<AudioSource>();

        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume;

        if (musicClip != null)
            musicSource.clip = musicClip;

        // Müzik kaynağından AYRI child — aynı AudioSource'ta çakışma olmasın
        Transform sfxChild = transform.Find("SfxSource");
        if (sfxChild == null)
        {
            var go = new GameObject("SfxSource");
            go.transform.SetParent(transform, false);
            _sfxSource = go.AddComponent<AudioSource>();
        }
        else
        {
            _sfxSource = sfxChild.GetComponent<AudioSource>();
            if (_sfxSource == null)
                _sfxSource = sfxChild.gameObject.AddComponent<AudioSource>();
        }

        _sfxSource.playOnAwake = false;
        _sfxSource.loop = false;
        _sfxSource.spatialBlend = 0f;
    }

    private void WireAllButtons()
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Button btn in buttons)
        {
            if (btn == null) continue;
            btn.onClick.AddListener(PlayButtonClick);
        }
    }

    public void ToggleMusic()
    {
        PlayButtonClick();
        IsMusicOn = !IsMusicOn;
        SaveMusicSetting();
        ApplyMusicState();
    }

    public void SetMusicOn(bool on)
    {
        IsMusicOn = on;
        SaveMusicSetting();
        ApplyMusicState();
    }

    public void SetSfxVolume(float volume)
    {
        SfxVolume = Mathf.Clamp01(volume);

        if (GameSaveManager.Instance != null)
            GameSaveManager.Instance.SfxVolume = SfxVolume;
    }

    public void PlayButtonClick()
    {
        if (buttonClickClip == null)
        {
            Debug.LogWarning("[MainMenuAudio] Button Click Clip atanmamış!", this);
            return;
        }

        if (SfxVolume <= 0.001f) return;

        if (_sfxSource != null)
            _sfxSource.PlayOneShot(buttonClickClip, SfxVolume);
        else
            AudioSource.PlayClipAtPoint(buttonClickClip, Vector3.zero, SfxVolume);
    }

    private void LoadSettings()
    {
        if (GameSaveManager.Instance == null)
            new GameObject("GameSaveManager (Auto)").AddComponent<GameSaveManager>();

        if (GameSaveManager.Instance == null) return;

        IsMusicOn = GameSaveManager.Instance.MusicOn;
        SfxVolume = GameSaveManager.Instance.SfxVolume;

        if (SfxVolume <= 0.001f)
            SfxVolume = 1f;
    }

    private void SaveMusicSetting()
    {
        if (GameSaveManager.Instance != null)
            GameSaveManager.Instance.MusicOn = IsMusicOn;
    }

    private void ApplyMusicState()
    {
        if (musicSource == null) return;

        if (musicClip != null && musicSource.clip != musicClip)
            musicSource.clip = musicClip;

        if (IsMusicOn)
        {
            musicSource.volume = musicVolume;

            if (musicSource.clip == null)
            {
                Debug.LogWarning("[MainMenuAudio] Music Clip atanmamış!", this);
                return;
            }

            if (!musicSource.isPlaying)
                musicSource.Play();
        }
        else
        {
            musicSource.Stop();
        }
    }
}
