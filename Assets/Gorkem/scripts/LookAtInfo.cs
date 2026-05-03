using UnityEngine;

/// <summary>
/// Bu objeye bakıldığında ekranda gösterilecek metni tutar.
/// Sahnedeki herhangi bir Collider'lı objeye eklenebilir.
/// </summary>
public class LookAtInfo : MonoBehaviour
{
    [Tooltip("Bu objeye bakıldığında ekranda gösterilecek metin.")]
    [TextArea(1, 4)]
    [SerializeField] private string message = "Bir obje";

    public string Message => message;
}
