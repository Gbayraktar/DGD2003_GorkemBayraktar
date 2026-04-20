using UnityEngine;
using UnityEngine.InputSystem;

public class CameraSwitcher : MonoBehaviour
{
    [Header("Cinemachine Kameralar")]
    [Tooltip("Sahnedeki 3 Cinemachine kamerayı sırayla buraya sürükle.")]
    [SerializeField] private GameObject[] cameras;

    [Header("Ayarlar")]
    [Tooltip("Oyun başladığında hangi kamera aktif olsun (0 = ilki)")]
    [SerializeField] private int startIndex = 0;

    private int _currentIndex;

    private void Start()
    {
        if (cameras == null || cameras.Length == 0)
        {
            Debug.LogWarning("CameraSwitcher: Listeye hiç kamera atanmamış.");
            return;
        }

        _currentIndex = Mathf.Clamp(startIndex, 0, cameras.Length - 1);
        UpdateActiveCamera();
    }

    private void Update()
    {
        if (Keyboard.current == null) return;
        if (cameras == null || cameras.Length == 0) return;

        if (Keyboard.current.cKey.wasPressedThisFrame)
            SwitchToNext();
    }

    private void SwitchToNext()
    {
        _currentIndex = (_currentIndex + 1) % cameras.Length;
        UpdateActiveCamera();
    }

    private void UpdateActiveCamera()
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] == null) continue;
            cameras[i].SetActive(i == _currentIndex);
        }
    }
}
