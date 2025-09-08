using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Slates.Networking.Input
{
    public class NetworkInputManager : SimulationBehaviour, IBeforeUpdate, INetworkRunnerCallbacks
    {
        private InputAction moveAction,
                            lookAction,
                            jumpAction,
                            selectAction,
                            interactAction,
                            escapeAction;

        private NetworkInputData _input;
        private bool _reset = true;

        private void Awake()
        {
            moveAction = InputSystem.actions.FindAction("Move");
            lookAction = InputSystem.actions.FindAction("Look");

            jumpAction = InputSystem.actions.FindAction("Jump");
            selectAction = InputSystem.actions.FindAction("Select");
            interactAction = InputSystem.actions.FindAction("Interact");
            escapeAction = InputSystem.actions.FindAction("Escape");
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
                buttons.Set((int)InputButtons.Escape, escapeAction.IsPressed());
            }
            else
            {
                // capture all
                _input.direction += moveAction.ReadValue<Vector2>();
                _input.look += lookAction.ReadValue<Vector2>();

                buttons.Set((int)InputButtons.Jump, jumpAction.IsPressed());
                buttons.Set((int)InputButtons.Select, selectAction.IsPressed());
                buttons.Set((int)InputButtons.Interact, interactAction.IsPressed());
                buttons.Set((int)InputButtons.Escape, escapeAction.IsPressed());
            }
            _input.buttons = new NetworkButtons(_input.buttons.Bits | buttons.Bits);
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            _input.direction.Normalize();
            _reset = true;

            input.Set(_input);
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
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    }
}