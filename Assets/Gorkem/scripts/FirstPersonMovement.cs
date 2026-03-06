using UnityEngine;

public class FirstPersonMovement : MonoBehaviour
{
    [Header("Hareket")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Bakış")]
    [SerializeField] private float mouseSensitivity = 200f;
    [SerializeField] private float verticalClamp = 80f;

    private float _horizontalRotation = 0f;
    private float _verticalRotation = 0f;

    private void Start()
    {
        _horizontalRotation = transform.eulerAngles.y;
        _verticalRotation = transform.eulerAngles.x;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleLook();
        HandleMovement();
        HandleCursorToggle();
    }

    private void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        _horizontalRotation += mouseX;
        _verticalRotation  -= mouseY;
        _verticalRotation   = Mathf.Clamp(_verticalRotation, -verticalClamp, verticalClamp);

        transform.rotation = Quaternion.Euler(_verticalRotation, _horizontalRotation, 0f);
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical   = Input.GetAxis("Vertical");

        Vector3 forward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        Vector3 right   = new Vector3(transform.right.x,   0f, transform.right.z).normalized;

        Vector3 move = (forward * vertical + right * horizontal) * moveSpeed * Time.deltaTime;
        transform.position += move;
    }

    private void HandleCursorToggle()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
