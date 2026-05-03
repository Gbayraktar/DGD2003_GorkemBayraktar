using UnityEngine;
using TMPro;
using System;

public class GameTimer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Ayarlar")]
    [SerializeField] private float totalSeconds = 90f;

    [Header("Uyarı (Son saniyeler)")]
    [SerializeField] private float warningThreshold = 30f;
    [SerializeField] private float blinkSpeed = 4f;
    [SerializeField] private Color warningColorA = Color.red;
    [SerializeField] private Color warningColorB = Color.white;
    [SerializeField] private Color normalColor   = Color.white;

    private float _timeLeft;
    private bool  _isRunning = true;

    public static event Action OnTimeUp;

    private void Start()
    {
        _timeLeft = totalSeconds;

        if (timerText != null)
            timerText.color = normalColor;
    }

    private void Update()
    {
        if (!_isRunning) return;

        _timeLeft -= Time.deltaTime;

        if (_timeLeft <= 0f)
        {
            _timeLeft  = 0f;
            _isRunning = false;
            OnTimeUp?.Invoke();
            Debug.Log("Süre doldu!");
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(_timeLeft / 60f);
        int seconds = Mathf.FloorToInt(_timeLeft % 60f);

        timerText.text = $"{minutes:00}:{seconds:00}";

        if (_timeLeft <= warningThreshold && _timeLeft > 0f)
        {
            float t = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            timerText.color = Color.Lerp(warningColorA, warningColorB, t);
        }
        else
        {
            timerText.color = normalColor;
        }
    }
}
