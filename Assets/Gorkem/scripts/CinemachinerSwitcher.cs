using UnityEngine;
using Cinemachine;
using System.Collections.Generic;

public class CameraSwitcher : MonoBehaviour
{
    public List<CinemachineVirtualCamera> cameras; // Kameralarý buraya sürükle
    private int currentIndex = 0;

    void Update()
    {
        // "C" tuþuna basýldýðýnda deðiþtir (Ýstediðin tuþu atayabilirsin)
        if (Input.GetKeyDown(KeyCode.C))
        {
            SwitchCamera();
        }
    }

    void SwitchCamera()
    {
        // Mevcut kameranýn önceliðini düþür
        cameras[currentIndex].Priority = 10;

        // Bir sonraki kameraya geç (Liste sonuna gelince baþa dön)
        currentIndex = (currentIndex + 1) % cameras.Count;

        // Yeni kameranýn önceliðini yükselt
        cameras[currentIndex].Priority = 20;
    }
}