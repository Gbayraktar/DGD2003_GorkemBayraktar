using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float walkSpeed = 5f;
    public float gravity = -9.81f;
    
    [Header("Bakış Ayarları")]
    public float mouseSensitivity = 100f;
    
    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        // Fareyi ekrana kitle
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // --- BAKIŞ MANTIĞI (Mouse Look) ---
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Yukarı-Aşağı bakış (Kameranın kendi X ekseni)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        
        // Sağa-Sola bakış (Kameranın dünya Y ekseni)
        // 'transform.eulerAngles' kullanarak hem sağa sola hem yukarı aşağı döneriz
        transform.localRotation = Quaternion.Euler(xRotation, transform.localRotation.eulerAngles.y + mouseX, 0f);

        // --- HAREKET MANTIĞI (Movement) ---
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Kameranın baktığı yöne göre hareket et
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * walkSpeed * Time.deltaTime);

        // Yer çekimi
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}