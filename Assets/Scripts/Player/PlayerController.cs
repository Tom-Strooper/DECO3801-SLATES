using Fusion;
using Fusion.Addons.KCC;
using Slates.Camera;
using Slates.Networking.Input;
using Slates.PuzzleInteractions.Controllers;
using Slates.PuzzleInteractions.Selection;
using UnityEngine;

namespace Slates.Player
{
    [RequireComponent(typeof(KCC))]
    public class PlayerController : NetworkBehaviour
    {
        private KCC _controller;

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

        private static Vector2 _antiJitterDistance = new Vector2(0.1f, 0.1f);

        private EscMenu _escMenu = null;

        private void Awake()
        {
            _controller = GetComponent<KCC>();
            _escMenu = GameObject.Find("Esc Menu Canvas").GetComponent<EscMenu>();
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

            // Apply smoothing
            _controller.Settings.AntiJitterDistance = _antiJitterDistance;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            // Unbind the camera singleton
            CameraController.Instance.UnbindFromHead(_head);
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData data))
            {
                // Handle Escape menu
                if (data.buttons.WasPressed(PreviousButtons, (int)InputButtons.Escape))
                {
                    if (_escMenu.IsEnabled())
                    {
                        // lock camera, hide mouse, menu disappears
                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;
                        _escMenu.Disappear();
                    }
                    else
                    {
                        // release camera, visible mouse, menu appears
                        // no change to other data inputs, since only escapeAction can be registered while cursor isn't locked
                        Cursor.lockState = CursorLockMode.Confined;
                        Cursor.visible = true;
                        _escMenu.Appear();
                    }
                }
                if (!_escMenu.IsEnabled() && Cursor.visible)
                {
                    // player closed the escape menu with button instead of key press
                    // lock the camera and hide the mouse
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }


                // Normalise direction to prevent wild movement input
                data.direction.Normalize();

                _verticalVelocity -= _gravity * Runner.DeltaTime;
                if (_controller.Data.IsGrounded && _verticalVelocity <= 0.0f)
                {
                    if (data.buttons.WasPressed(PreviousButtons, (int)InputButtons.Jump)) _verticalVelocity = 10.0f;
                    else _verticalVelocity = -1.0f;
                }

                Vector3 targetVelocity = (_controller.Transform.forward * data.direction.y + _controller.Transform.right * data.direction.x) * _maxMovementSpeed;
                Vector3 velocity = _controller.Data.RealVelocity + (targetVelocity - _controller.Data.RealVelocity) * _accelerationCoefficient * Runner.DeltaTime;

                velocity.y = _verticalVelocity;

                _controller.SetKinematicVelocity(velocity);

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
                if (data.buttons.WasPressed(PreviousButtons, (int)InputButtons.Select)) {Select();}

                // Update values
                PreviousButtons = data.buttons;
            }

            if (transform.position.y < -20.0f)
            {
                _controller.SetPosition(Vector3.up);
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
            if (!HasInputAuthority) return;

            // If we are holding something, drop it
            if (_held is not null)
            {
                RPC_RequestDeselect((NetworkBehaviour)_held, this);
                return;
            }

            // Perform a raycast, ignoring the player's rigidbody, to see if the player is selecting anything in range
            RaycastHit hit;
            if (!Physics.Raycast(_head.position, _head.forward, out hit, _maxSelectionDistance, _interactionLayerMask.value)) return;

            if (hit.collider.attachedRigidbody?.GetComponent<ISelectable>() is not ISelectable selectable) return;
            if (selectable.IsSelected) return;

            RPC_RequestSelect((NetworkBehaviour)selectable, this);
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_RequestSelect(NetworkBehaviour selectable, PlayerController selector)
        {
            if (selectable is not ISelectable s) return;
            s.RPC_OnSelected(selector);
        }
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_RequestDeselect(NetworkBehaviour selectable, PlayerController selector)
        {
            if (selectable is not ISelectable s) return;
            s.RPC_OnDeselected(selector);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.StateAuthority | RpcTargets.InputAuthority)]
        public void RPC_Grab(MoveableObjectController body)
        {
            if (_held is not null) RPC_RequestDeselect((NetworkBehaviour)_held, this);

            _held = body;

            _held.RB.useGravity = false;
            _held.RB.isKinematic = true;
        }
        [Rpc(RpcSources.StateAuthority, RpcTargets.StateAuthority | RpcTargets.InputAuthority)]
        public void RPC_Drop(MoveableObjectController body)
        {
            if (_held is null || _held != (ISelectable)body) return;

            _held.RB.useGravity = true;
            _held.RB.isKinematic = false;

            _held = null;
        }
    }
}
