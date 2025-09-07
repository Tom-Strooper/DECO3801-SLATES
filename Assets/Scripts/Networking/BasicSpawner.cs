using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Slates.Networking
{
    public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
    {
        private NetworkRunner _runner;

        [SerializeField] private NetworkPrefabRef _playerPrefab;
        [SerializeField] private Text _lobbyCodeOnHUD;

        private BackgroundInfo _backgroundInfo;

        private InputAction hostGameAction, joinGameAction;

        private Dictionary<PlayerRef, NetworkObject> _players = new Dictionary<PlayerRef, NetworkObject>();
        private string _lobbyCode;
        private GameMode _playerMode;

        private void Awake()
        {
            _backgroundInfo = GameObject.Find("Background Info").GetComponent<BackgroundInfo>();
            _lobbyCode = _backgroundInfo.GetLobbyCode();
            _playerMode = _backgroundInfo.GetPlayerMode();

            StartGame();
        }

        private void Update()
        {
            if (_runner is not null) return;
            if (_runner is null) Debug.Log("BasicSpawner: null runner");
            return;
        }

        private async void StartGame()
        {
            // Create a Fusion NetworkRunner
            _runner = gameObject.AddComponent<NetworkRunner>();

            // Indicate to the NetworkRunner that this component will be providing user input
            // (i.e., that a player will be playing in this game)
            _runner.ProvideInput = true;

            // Load a reference to the current scene into a NetworkSceneInfo, to share with the NetworkRunner
            SceneRef scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);

            NetworkSceneInfo sceneInfo = new NetworkSceneInfo();
            if (scene.IsValid) sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);

            if (_playerMode == GameMode.Host)
            {
                Debug.Log("Using lobby code: " + _lobbyCode);
            }
            else if (_playerMode == GameMode.Client)
            {
                Debug.Log("Finding host " + _lobbyCode);
            }

            _lobbyCodeOnHUD.text = "Lobby Code: " + _lobbyCode;

            // Start or join (depends on game mode) a game session
            await _runner.StartGame(new StartGameArgs()
            {
                GameMode = _playerMode,
                SessionName = _lobbyCode,
                Scene = scene, // This only matters if the game is started by a host - clients must use host's scene
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
            });
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (_runner.IsServer)
            {
                // Create a unique spawn position for the player
                Vector3 spawnPosition = new Vector3((player.RawEncoded % runner.Config.Simulation.PlayerCount) * 3.0f, 1.0f, 0.0f);
                NetworkObject playerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
                // Keep track of the players
                _players.Add(player, playerObject);
                _runner.SetPlayerObject(player, playerObject);
            }
        }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log("BasicSpawner: OnPlayerLeft");
            if (_players.ContainsKey(player))
            {
                // Despawn the player
                runner.Despawn(_players[player]);
                _players.Remove(player);
                // Free the mouse
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        public NetworkRunner GetRunner()
        {
            return _runner;
        }

        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            Debug.Log("BasicSpawner.OnShutdown: Connection to host " + _lobbyCode + " failed!");
            SceneManager.LoadScene(1);
        }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    }
}
