using UnityEngine;

namespace Slates.Camera
{
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

        public bool Raycast(out RaycastHit hit, float distance, LayerMask? mask = null)
        {
            if (mask is null) return Physics.Raycast(transform.position, transform.forward, out hit, distance);
            return Physics.Raycast(transform.position, transform.forward, out hit, distance, (LayerMask)mask);
        }
        public Vector3 RaycastPoint(float distance, LayerMask? mask = null)
        {
            if (Raycast(out RaycastHit hit, distance, mask))
            {
                return hit.point;
            }
            else
            {
                return transform.position + transform.forward * distance;
            }
        }
    }
}
