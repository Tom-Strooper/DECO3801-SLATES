using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; } = null;

    private void Awake()
    {
        if (Instance is null) Instance = this;
        else Destroy(this);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void BindToHead(Transform head)
    {
        // Parent the camera to the player
        transform.SetParent(head);

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void UnbindFromHead(Transform head)
    {
        // Parent the camera to the root transform of the scene
        transform.SetParent(head.root);
    }
}
