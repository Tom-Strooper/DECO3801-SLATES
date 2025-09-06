using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Addons.Physics;
using Fusion.Sockets;
using Slates.Networking.Input;
using Slates.Utility;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Slates.Networking
{
    [RequireComponent(typeof(NetworkRunner), typeof(RunnerSimulatePhysics3D), typeof(NetworkInputManager))]
    public class NetworkGameManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        private NetworkRunner _runner;

        private InputAction _debugHost, _debugJoin;

        private void Awake()
        {
            _runner = GetComponent<NetworkRunner>();

            _debugHost = InputSystem.actions.FindAction("Host");
            _debugJoin = InputSystem.actions.FindAction("Join");
        }

        private void Update()
        {
            if (_runner.IsRunning) return;

            if (_debugHost.WasPressedThisFrame()) StartGameAsHost();
            else if (_debugJoin.WasPressedThisFrame()) StartGameAsClient("Test Session");
        }

        public void StartGameAsHost()
        {
            // TODO - Generate session name/code
            string sessionName = "Test Session";
            _ = StartGame(true, sessionName);
        }
        public void StartGameAsClient(string sessionName)
        {
            _ = StartGame(false, sessionName);
        }

        private async Task StartGame(bool asHost, string sessionName)
        {
            _runner.ProvideInput = true;

            // Reference current scene
            SceneRef scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
            NetworkSceneInfo info = new NetworkSceneInfo();

            if (!scene.IsValid)
            {
                // We could not load the scene, so exit from the start game method
                Debug.Log("Scene reference invalid, destroying network runner!");
                return;
            }

            info.AddSceneRef(scene);

            // Finally, start the game
            StartGameArgs args = new StartGameArgs();

            args.GameMode = asHost ? GameMode.Host : GameMode.Client;
            args.SessionName = sessionName;
            args.Scene = scene;
            args.SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();

            if (asHost)
            {
                args.PlayerCount = Constants.MaxPlayers;
            }

            StartGameResult result = await _runner.StartGame(args);
            Debug.Log(result);
            if (result.Ok)
            {
                // TODO - Hide main menu
                return;
            }

            // TODO - Error/feedback reporting (UI)
        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
    }
}