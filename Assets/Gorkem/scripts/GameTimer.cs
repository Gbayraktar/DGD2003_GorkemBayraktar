using UnityEngine;
using TMPro;
using System;

public class GameTimer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Ayarlar")]
    [SerializeField] private float totalSeconds = 90f;

    private float _timeLeft;
    private bool  _isRunning = true;

    public static event Action OnTimeUp;

    private void Start()
    {
        _timeLeft = totalSeconds;
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
    }
}
