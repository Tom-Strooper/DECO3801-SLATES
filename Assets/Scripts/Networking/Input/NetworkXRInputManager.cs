using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using Slates.Camera;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Slates.Networking.Input
{
    public class NetworkXRInputManager : SimulationBehaviour, IBeforeUpdate, INetworkRunnerCallbacks
    {
        public bool TrackInput { get; set; } = false;

        [SerializeField] private InputActionAsset _actions;

        private InputAction _lookAction,
                            _summonAction,
                            _grabAction,
                            _pinchAction;

        private NetworkXRInputData _input;
        private bool _reset = true;

        private Transform _rightHand = null;

        private void Awake()
        {
            InputActionMap xrPlayerInputActions = _actions.FindActionMap("XR Player");

            _lookAction = xrPlayerInputActions.FindAction("Look");

            _summonAction = xrPlayerInputActions.FindAction("Summon");
            _grabAction = xrPlayerInputActions.FindAction("Grab");
            _pinchAction = xrPlayerInputActions.FindAction("Pinch");
        }

        public void SetRightHand(Transform rightHand) => _rightHand = rightHand;

        public void BeforeUpdate()
        {
            if (_reset)
            {
                _input = new NetworkXRInputData();
                _reset = false;
            }

            NetworkButtons buttons = new NetworkButtons();

            if (_rightHand is null)
            {
                _input.interactionOrigin = CameraController.Instance.transform.position;
                _input.interactionDirection = CameraController.Instance.transform.forward;
            }
            else
            {
                _input.interactionOrigin = _rightHand.position;
                _input.interactionDirection = _rightHand.forward;
            }

            _input.look = _lookAction.ReadValue<Vector2>();

            // TODO - Implement this using the XR hand gesture
            bool isPalmUp = false;

            buttons.Set((int)XRInputButtons.Summon, _summonAction.IsPressed() || isPalmUp);
            buttons.Set((int)XRInputButtons.Grab, _grabAction.IsPressed());
            buttons.Set((int)XRInputButtons.Pinch, _pinchAction.IsPressed());

            _input.buttons = new NetworkButtons(_input.buttons.Bits | buttons.Bits);
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            _input.interactionDirection.Normalize();
            _reset = true;

            if (!TrackInput) return;
            input.Set(_input);
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
        }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
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