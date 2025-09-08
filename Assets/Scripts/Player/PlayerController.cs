using Fusion;
using Fusion.Addons.KCC;
using Slates.Camera;
using Slates.Networking;
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
            _controller = GetComponent<KCC>();
        }

        public override void Spawned()
        {
            if (!HasInputAuthority) return;

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
            if (GetInput(out NetworkInputData data))
            {
                // Handle Escape menu
                if (data.buttons.WasPressed(PreviousButtons, (int)InputButtons.Pause))
                {
                    if (NetworkGameManager.Instance.IsPaused)
                    {
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;

                        NetworkGameManager.Instance.UnpauseGame();
                    }
                    else
                    {
                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;

                        NetworkGameManager.Instance.PauseGame();
                    }
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

                _controller.SetDynamicVelocity(velocity);

                _controller.AddLookRotation(-data.look.y * _sensitivity * Runner.DeltaTime, data.look.x * _sensitivity * Runner.DeltaTime);
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

                // Update button values
                PreviousButtons = data.buttons;
            }

            if (transform.position.y < -20.0f)
            {
                _controller.SetPosition(NetworkSpawner.Instance.NonVRSpawn.position);
            }
        }

        public override void Render()
        {
            UpdateCameraRotation();
        }

        private void UpdateCameraRotation()
        {
            _head.transform.localRotation = Quaternion.Euler(_controller.GetLookRotation().x, 0.0f, 0.0f);
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
            if (!CameraController.Instance.Raycast(out RaycastHit hit, _maxSelectionDistance, _interactionLayerMask)) return;

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
