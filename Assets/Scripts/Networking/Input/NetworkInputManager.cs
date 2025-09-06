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
        private InputAction moveAction,
                            lookAction,
                            jumpAction,
                            selectAction,
                            interactAction,
                            pauseAction;

        private NetworkInputData _input;
        private bool _reset = true;

        private void Awake()
        {
            moveAction = InputSystem.actions.FindAction("Move");
            lookAction = InputSystem.actions.FindAction("Look");

            jumpAction = InputSystem.actions.FindAction("Jump");
            selectAction = InputSystem.actions.FindAction("Select");
            interactAction = InputSystem.actions.FindAction("Interact");

            pauseAction = InputSystem.actions.FindAction("Pause");
        }

        public void BeforeUpdate()
        {
            if (_reset)
            {
                _input = new NetworkInputData();
                _reset = false;
            }

            // Show/hide cursor
            if (pauseAction.WasPressedThisFrame())
            {
                // TODO - Pause/unpause
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }

            // Only capture input when mouse is locked (i.e., not interacting w/ menus, clicked outside of game, etc)
            if (Cursor.lockState != CursorLockMode.Locked) return;

            NetworkButtons buttons = new NetworkButtons();

            _input.direction += moveAction.ReadValue<Vector2>();
            _input.look += lookAction.ReadValue<Vector2>();

            buttons.Set((int)InputButtons.Jump, jumpAction.IsPressed());
            buttons.Set((int)InputButtons.Select, selectAction.IsPressed());
            buttons.Set((int)InputButtons.Interact, interactAction.IsPressed());

            _input.buttons = new NetworkButtons(_input.buttons.Bits | buttons.Bits);
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            _input.direction.Normalize();
            _reset = true;

            input.Set(_input);
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