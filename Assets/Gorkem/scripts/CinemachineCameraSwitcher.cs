using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CinemachineCameraSwitcher : MonoBehaviour
{
    [Header("Cinemachine Virtual Kameralar")]
    [Tooltip("Numpad 1/2/3 ile bu sıraya göre geçiş yapılır.")]
    [SerializeField] private CinemachineVirtualCamera[] cameras = new CinemachineVirtualCamera[3];

    [Header("Öncelik Ayarları")]
    [SerializeField] private int activePriority   = 20;
    [SerializeField] private int inactivePriority = 0;

    [Header("Başlangıç")]
    [Tooltip("Sahne başlarken aktif olacak kameranın index'i (0, 1, 2).")]
    [SerializeField] private int startIndex = 0;

    private int _currentIndex = -1;

    private void Start()
    {
        if (cameras == null || cameras.Length == 0)
        {
            Debug.LogWarning("[CinemachineCameraSwitcher] Kamera listesi boş.");
            enabled = false;
            return;
        }

        SwitchTo(Mathf.Clamp(startIndex, 0, cameras.Length - 1));
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.numpad1Key.wasPressedThisFrame) SwitchTo(0);
        else if (kb.numpad2Key.wasPressedThisFrame) SwitchTo(1);
        else if (kb.numpad3Key.wasPressedThisFrame) SwitchTo(2);
    }

    public void SwitchTo(int index)
    {
        if (cameras == null || index < 0 || index >= cameras.Length) return;
        if (cameras[index] == null)
        {
            Debug.LogWarning($"[CinemachineCameraSwitcher] Slot {index} boş.");
            return;
        }
        if (index == _currentIndex) return;

        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] == null) continue;
            cameras[i].Priority = (i == index) ? activePriority : inactivePriority;
        }

        _currentIndex = index;
    }
}
