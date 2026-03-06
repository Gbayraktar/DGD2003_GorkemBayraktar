using UnityEngine;

public class EyeLookAt : MonoBehaviour
{
    [Header("Hedef")]
    [SerializeField] private Transform target;

    [Header("Ayarlar")]
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private bool lockXRotation = false;
    [SerializeField] private bool lockZRotation = true;

    private void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) target = player.transform;
        }
    }

    private void Update()
    {
        if (target == null) return;

        Vector3 direction = target.position - transform.position;

        if (lockXRotation) direction.y = 0f;
        if (lockZRotation) direction.z = Mathf.Abs(direction.z);

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}
