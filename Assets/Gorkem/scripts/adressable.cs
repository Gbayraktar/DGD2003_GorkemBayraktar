using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressablePrefabLoader : MonoBehaviour
{
    [SerializeField] private string address = "SecurityCameraPrefab";
    [SerializeField] private Transform spawnPoint;

    private GameObject _spawned;
    private AsyncOperationHandle<GameObject> _handle;

    public void Spawn()
    {
        _handle = Addressables.InstantiateAsync(address, spawnPoint.position, spawnPoint.rotation);
        _handle.Completed += op =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
                _spawned = op.Result;
        };
    }

    private void OnDestroy()
    {
        if (_spawned != null)
            Addressables.ReleaseInstance(_spawned);
    }
}