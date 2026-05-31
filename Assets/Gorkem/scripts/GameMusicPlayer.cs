using UnityEngine;

/// <summary>
/// Oyun sahnesinde arka plan müziği çalar.
/// ESC menüsündeki Volume slider bu müziğin sesini kontrol eder (GameSaveManager.MasterVolume).
/// </summary>
public class GameMusicPlayer : MonoBehaviour
{
    public static GameMusicPlayer Instance { get; private set; }

    [Header("Müzik — mp3/wav/flac dosyasını buraya sürükle")]
    [SerializeField] private AudioClip musicClip;

    [Tooltip("Slider 1 iken müziğin maksimum sesi")]
    [SerializeField] private float maxMusicVolume = 0.5f;

    private AudioSource _source;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        SetupSource();
        EnsureSaveManager();
    }

    private void Start()
    {
        ApplySavedSettings();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void SetupSource()
    {
        _source = GetComponent<AudioSource>();
        if (_source == null)
            _source = gameObject.AddComponent<AudioSource>();

        _source.loop = true;
        _source.playOnAwake = false;
        _source.spatialBlend = 0f;
        _source.volume = maxMusicVolume;

        if (musicClip != null)
            _source.clip = musicClip;
    }

    /// <summary>ESC menüsü volume slider'ından çağrılır (0-1).</summary>
    public void SetVolumeMultiplier(float multiplier)
    {
        if (_source == null) return;

        float v = NormalizeVolume(multiplier);
        _source.volume = maxMusicVolume * v;

        if (GameSaveManager.Instance != null)
            GameSaveManager.Instance.MasterVolume = v;
    }

    public void ApplySavedSettings()
    {
        if (_source == null) SetupSource();

        if (musicClip != null)
            _source.clip = musicClip;

        if (_source.clip == null)
        {
            Debug.LogWarning("[GameMusicPlayer] Music Clip atanmamış! Inspector'dan müzik dosyasını sürükle.", this);
            return;
        }

        float volMult = GameSaveManager.Instance != null
            ? NormalizeVolume(GameSaveManager.Instance.MasterVolume)
            : 1f;

        _source.volume = maxMusicVolume * volMult;

        if (!_source.isPlaying)
            _source.Play();
    }

    /// <summary>Slider 0 kayıtlıysa sessiz kalmasın diye varsayılan 1.</summary>
    private static float NormalizeVolume(float value)
    {
        float v = Mathf.Clamp01(value);
        return v <= 0.001f ? 1f : v;
    }

    private static void EnsureSaveManager()
    {
        if (GameSaveManager.Instance != null) return;
        new GameObject("GameSaveManager (Auto)").AddComponent<GameSaveManager>();
    }
}
