using Fusion;
using Fusion.Addons.SimpleKCC;
using Slates.Camera;
using Slates.Networking.Input;
using Slates.PuzzleInteractions.Selection;
using UnityEngine;

namespace Slates.Player
{
    [RequireComponent(typeof(SimpleKCC))]
    public class PlayerController : NetworkBehaviour
    {
        private SimpleKCC _controller;

        // Networked values
        [Networked] private NetworkButtons PreviousButtons { get; set; }

        [Header("Camera Settings")]
        [SerializeField] private Transform _head;
        [SerializeField] private float _sensitivity = 50.0f;

        private float _xRotation = 0.0f;

        [Header("Movement Settings")]
        [SerializeField] private float _gravity = 20.0f;
        [SerializeField] private float _maxMovementSpeed = 10.0f;
        [SerializeField] private float _accelerationCoefficient = 10.0f;

        private float _verticalVelocity = 0.0f;

        [Header("Interaction Settings")]
        [SerializeField] private float _maxSelectionDistance;
        [SerializeField] private Transform _heldObjectPoint;
        [SerializeField] private LayerMask _interactionLayerMask;

        private ISelectable _held = null;

        private void Awake()
        {
            _controller = GetComponent<SimpleKCC>();
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
                if (_controller.IsGrounded && _verticalVelocity <= 0.0f)
                {
                    if (data.buttons.WasPressed(PreviousButtons, (int)InputButtons.Jump)) _verticalVelocity = 10.0f;
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

                UpdateCameraRotation();

                // Update positioning of held object
                if (_held is not null)
                {
                    // Match the speed of the player
                    _held.RB.MovePosition(_heldObjectPoint.position);
                    _held.RB.MoveRotation(_heldObjectPoint.rotation);
                }

                // Handle select/deselect
                if (data.buttons.WasPressed(PreviousButtons, (int)InputButtons.Select)) { Select(); }

                // Update values
                PreviousButtons = data.buttons;
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

        private void Select()
        {
            // If we are holding something, drop it
            if (_held is not null)
            {
                _held.OnDeselected(this);
                return;
            }

            Debug.DrawRay(_head.position, _head.forward * _maxSelectionDistance, Color.red, 1.0f);

            // Perform a raycast, ignoring the player's rigidbody, to see if the player is selecting anything in range
            RaycastHit hit;
            if (!Physics.Raycast(_head.position,
                                 _head.forward,
                                 out hit,
                                 _maxSelectionDistance,
                                 _interactionLayerMask.value)) return;

            if (hit.collider.attachedRigidbody?.GetComponent<ISelectable>() is not ISelectable selectable) return;
            if (selectable.IsSelected) return;

            selectable.OnSelected(this);
        }

        public void Grab(ISelectable body)
        {
            if (_held is not null) _held.OnDeselected(this);

            _held = body;

            _held.RB.useGravity = false;
            _held.RB.isKinematic = true;
        }
        public void Drop(ISelectable body)
        {
            if (_held is null || _held != body) return;

            _held.RB.useGravity = true;
            _held.RB.isKinematic = false;

            _held = null;
        }
    }
}
