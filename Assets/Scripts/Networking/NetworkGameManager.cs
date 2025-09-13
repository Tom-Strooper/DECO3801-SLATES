using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Addons.Physics;
using Fusion.Sockets;
using Slates.Networking.Input;
using Slates.UI;
using Slates.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Slates.Networking
{
    [
        RequireComponent(typeof(NetworkRunner)),
        RequireComponent(typeof(RunnerSimulatePhysics3D)),
        RequireComponent(typeof(NetworkInputManager)),
        RequireComponent(typeof(NetworkXRInputManager))
    ]
    public class NetworkGameManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        public static NetworkGameManager Instance { get; private set; } = null;

        public BackgroundInfo Info { get; } = new BackgroundInfo();

        public bool IsPaused { get; private set; } = false;

        private NetworkRunner _runner;

        private NetworkInputManager _input;
        private NetworkXRInputManager _xrInput;

        [SerializeField] private Menu _mainMenu;
        [SerializeField] private PauseMenu _pauseMenu;

        private void Awake()
        {
            if (Instance is null) Instance = this;
            else Destroy(gameObject);

            _runner = GetComponent<NetworkRunner>();

            _input = GetComponent<NetworkInputManager>();
            _xrInput = GetComponent<NetworkXRInputManager>();

            // Inputs will be enabled upon game start
            _input.TrackInput = false;
            _xrInput.TrackInput = false;

            // Initialise menus
            _mainMenu.gameObject.SetActive(true);
            _pauseMenu.gameObject.SetActive(false);

            // Initialise settings
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 120;
        }

        public void StartGame() => _ = StartGameAsync();
        private async Task StartGameAsync()
        {
            _runner.ProvideInput = true;

            // Reference current scene
            SceneRef scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
            NetworkSceneInfo info = new NetworkSceneInfo();

            if (!scene.IsValid)
            {
                // We could not load the scene, so exit from the start game method
                Debug.Log("Scene reference invalid!");
                return;
            }

            info.AddSceneRef(scene);

            // Enable the appropriate input
            if (Info.PlayerMode == GameMode.Host) _xrInput.TrackInput = true;
            else _input.TrackInput = true;

            // Finally, start the game
            StartGameArgs args = new StartGameArgs();

            args.GameMode = Info.PlayerMode;
            args.SessionName = Info.LobbyCode;
            args.PlayerCount = Constants.MaxPlayers;
            args.Scene = scene;
            args.SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();

            StartGameResult result = await _runner.StartGame(args);

            if (result.Ok)
            {
                _mainMenu.gameObject.SetActive(false);
                return;
            }

            // TODO - Error/feedback reporting (UI)
        }

        public void PauseGame()
        {
            _pauseMenu.gameObject.SetActive(true);
        }
        public void UnpauseGame()
        {
            _pauseMenu.gameObject.SetActive(true);
        }

        public void QuitGame() => Application.Quit();

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            _input.TrackInput = false;
            _xrInput.TrackInput = false;

            // Show main menu
            _mainMenu.gameObject.SetActive(true);
        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
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