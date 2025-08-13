using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;

[RequireComponent(typeof(SimpleKCC))]
public class Player : NetworkBehaviour
{
    private SimpleKCC _controller;

    [Header("Camera Settings")]
    [SerializeField] private Camera _camera;
    [SerializeField] private float _sensitivity = 50.0f;

    private float _xRotation = 0.0f;

    [Header("Movement Settings")]
    [SerializeField] private float _gravity = 20.0f;
    [SerializeField] private float _maxMovementSpeed = 10.0f;
    [SerializeField] private float _accelerationCoefficient = 10.0f;

    private float _verticalVelocity = 0.0f;

    private void Awake()
    {
        _controller = GetComponent<SimpleKCC>();

        // TODO - Gotta move this
        // Fix the mouse in the centre of the screen and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public override void FixedUpdateNetwork()
    {
        // TODO - Improve character controller
        if (GetInput(out NetworkInputData data))
        {
            // Normalise direction to prevent wild movement input
            data.direction.Normalize();

            _verticalVelocity -= _gravity * Runner.DeltaTime;
            if (_controller.IsGrounded)
            {
                if (data.buttons.IsSet((int)InputButtons.Jump)) _verticalVelocity = 10.0f;
                else _verticalVelocity = -1.0f;
            }

            Vector3 targetVelocity = (_controller.Transform.forward * data.direction.y + _controller.Transform.right * data.direction.x) * _maxMovementSpeed;
            Vector3 velocity = _controller.RealVelocity + (targetVelocity - _controller.RealVelocity) * _accelerationCoefficient * Runner.DeltaTime;

            velocity.y = _verticalVelocity;

            _controller.Move(velocity);

            // TODO - Fix camera jitter
            _controller.AddLookRotation(0.0f, data.look.x * _sensitivity * Runner.DeltaTime);

            _xRotation -= data.look.y * _sensitivity * Runner.DeltaTime;
            _xRotation = Mathf.Clamp(_xRotation, -80.0f, 85.0f);

            _camera.transform.localRotation = Quaternion.Euler(_xRotation, 0.0f, 0.0f);
        }
    }
}
