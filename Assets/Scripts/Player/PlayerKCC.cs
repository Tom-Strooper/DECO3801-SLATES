using Fusion;
using Fusion.Addons.KCC;
using UnityEngine;

[RequireComponent(typeof(KCC))]
public class PlayerKCC : NetworkBehaviour
{
    private KCC KCC;

    [Header("Camera Settings")]
    [SerializeField] private Transform _head;
    [SerializeField] private float _sensitivity = 50.0f;

    private float _xRotation = 0.0f;

    [Header("Movement Settings")]
    [SerializeField] private float _gravity = 20.0f;
    [SerializeField] private float _maxMovementSpeed = 10.0f;
    [SerializeField] private float _accelerationCoefficient = 10.0f;

    private float _verticalVelocity = 0.0f;

    private void Awake()
    {
        KCC = GetComponent<KCC>();
        QualitySettings.vSyncCount = 2;
    }

    public override void Spawned()
    {
        if (!HasInputAuthority) return;

        // TODO - Gotta move this
        // Fix the mouse in the centre of the screen and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Indicate to the camera that this player should be followed
        CameraController.Instance.BindToHead(_head);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        // Unbind the camera singleton
        CameraController.Instance.UnbindFromHead(_head);
    }

    public override void FixedUpdateNetwork()
    {
        // TODO - Improve character controller
        if (GetInput(out NetworkInputData data))
        {
            // Normalise direction to prevent wild movement input
            data.direction.Normalize();

            _verticalVelocity -= _gravity * Runner.DeltaTime;
            if (KCC.Data.IsGrounded)
            {
                if (data.buttons.IsSet((int)InputButtons.Jump)) _verticalVelocity = 10.0f;
                else _verticalVelocity = -1.0f;
            }

            Vector3 targetVelocity = (KCC.Transform.forward * data.direction.y + KCC.Transform.right * data.direction.x) * _maxMovementSpeed;
            Vector3 velocity = KCC.Data.RealVelocity + (targetVelocity - KCC.Data.RealVelocity) * _accelerationCoefficient * Runner.DeltaTime;

            velocity.y = _verticalVelocity;

            KCC.SetKinematicVelocity(velocity);

            // TODO - Fix camera jitter
            KCC.AddLookRotation(0.0f, data.look.x * _sensitivity * Runner.DeltaTime);

            _xRotation -= data.look.y * _sensitivity * Runner.DeltaTime;
            _xRotation = Mathf.Clamp(_xRotation, -80.0f, 85.0f);

            UpdateCameraRotation();
        }
    }

    public override void Render()
    {
        UpdateCameraRotation();
    }

    private void UpdateCameraRotation()
    {
        _head.transform.localRotation = Quaternion.Euler(_xRotation, 0.0f, 0.0f);
    }
}
