using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Slates.Networking.Input
{
    // Implementation based off MultiClimb Tutorial on Photon
    public class NetworkInputManager : SimulationBehaviour, IBeforeUpdate, INetworkRunnerCallbacks
    {
        public bool TrackInput { get; set; } = false;

        [SerializeField] private InputActionAsset _actions;

        private InputAction _moveAction,
                            _lookAction,
                            _jumpAction,
                            _selectAction,
                            _interactAction,
                            _pauseAction;

        private NetworkInputData _input;
        private bool _reset = true;

        private void Awake()
        {
            InputActionMap playerInputActions = _actions.FindActionMap("Player");

            // Bind actions
            _moveAction = playerInputActions.FindAction("Move");
            _lookAction = playerInputActions.FindAction("Look");

            _jumpAction = playerInputActions.FindAction("Jump");
            _selectAction = playerInputActions.FindAction("Select");
            _interactAction = playerInputActions.FindAction("Interact");

            _pauseAction = playerInputActions.FindAction("Pause");
        }

        public void BeforeUpdate()
        {
            if (_reset)
            {
                _input = new NetworkInputData();
                _reset = false;
            }

            // Only capture input when mouse is locked (i.e., not interacting w/ menus, clicked outside of game, etc)
            NetworkButtons buttons = new NetworkButtons();
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                // if mouse is not locked, we still want to capture Escape input to be able to close escape canvas
                buttons.Set((int)InputButtons.Pause, _pauseAction.IsPressed());
            }
            else
            {
                // capture all
                _input.direction += _moveAction.ReadValue<Vector2>();
                _input.look += _lookAction.ReadValue<Vector2>();

                buttons.Set((int)InputButtons.Jump, _jumpAction.IsPressed());
                buttons.Set((int)InputButtons.Select, _selectAction.IsPressed());
                buttons.Set((int)InputButtons.Interact, _interactAction.IsPressed());
                buttons.Set((int)InputButtons.Pause, _pauseAction.IsPressed());
            }

            _input.buttons = new NetworkButtons(_input.buttons.Bits | buttons.Bits);
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            _input.direction.Normalize();
            _reset = true;

            if (!TrackInput) return;

            input.Set(_input);

            // Prevents camera sway & abrupt sensitivity changes
            _input.look = Vector2.zero;
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (runner.LocalPlayer != player) return;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            // Show the cursor after disconnection from server
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    }
}