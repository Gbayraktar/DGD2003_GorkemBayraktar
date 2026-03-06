using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class ThirdPersonMovement : MonoBehaviour
{
    [Header("Hareket")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Fizik")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float jumpHeight = 1.5f;

    [Header("Referanslar")]
    [SerializeField] private Transform cameraTransform;

    private CharacterController _controller;
    private Animator _animator;
    private Vector3 _velocity;

    private static readonly int Speed        = Animator.StringToHash("speed");
    private static readonly int IsDancing    = Animator.StringToHash("isDancing");
    private static readonly int HappyTrigger = Animator.StringToHash("happy");
    private static readonly int SadTrigger   = Animator.StringToHash("sad");

    private bool _isDancing = false;
    private float _currentSpeed = 0f;

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        _animator   = GetComponent<Animator>();

        _animator.applyRootMotion = false;

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        float speed = HandleMovement();
        HandleJump();
        ApplyGravity();
        HandleAnimations(speed);
    }

    private float HandleMovement()
    {
        if (Keyboard.current == null) return 0f;

        float h = 0f, v = 0f;
        if (Keyboard.current.dKey.isPressed) h += 1f;
        if (Keyboard.current.aKey.isPressed) h -= 1f;
        if (Keyboard.current.wKey.isPressed) v += 1f;
        if (Keyboard.current.sKey.isPressed) v -= 1f;

        Vector3 inputDir = new Vector3(h, 0f, v).normalized;
        bool isMoving = inputDir.magnitude >= 0.1f;

        if (isMoving)
        {
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg
                                + cameraTransform.eulerAngles.y;

            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            Vector3 moveDir = targetRotation * Vector3.forward;
            _controller.Move(moveDir * moveSpeed * Time.deltaTime);
        }

        float targetSpeed = isMoving ? 1f : 0f;
        _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, Time.deltaTime * 8f);
        return _currentSpeed;
    }

    private void HandleJump()
    {
        if (Keyboard.current == null) return;

        if (_controller.isGrounded && Keyboard.current.spaceKey.wasPressedThisFrame)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    private void ApplyGravity()
    {
        if (_controller.isGrounded && _velocity.y < 0f)
            _velocity.y = -2f;

        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(new Vector3(0f, _velocity.y, 0f) * Time.deltaTime);
    }

    private void HandleAnimations(float speed)
    {
        if (Keyboard.current == null) return;

        bool isMoving = speed > 0.05f;

        // Dans: F tuşuyla aç/kapat, hareket edince veya ESC ile iptal et
        if (Keyboard.current.fKey.wasPressedThisFrame && !isMoving)
            _isDancing = !_isDancing;

        if (isMoving || Keyboard.current.escapeKey.wasPressedThisFrame)
            _isDancing = false;

        // Happy: E tuşuyla bir kere oynat
        if (Keyboard.current.eKey.wasPressedThisFrame && !_isDancing)
            _animator.SetTrigger(HappyTrigger);

        // Sad: R tuşuyla bir kere oynat
        if (Keyboard.current.rKey.wasPressedThisFrame && !_isDancing)
            _animator.SetTrigger(SadTrigger);

        _animator.SetFloat(Speed, speed);
        _animator.SetBool(IsDancing, _isDancing);
    }
}
