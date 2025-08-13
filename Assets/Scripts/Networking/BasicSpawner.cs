using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner _runner;

    [SerializeField] private Camera _camera;
    [SerializeField] private NetworkPrefabRef _playerPrefab;

    InputAction moveAction, lookAction, jumpAction, hostGameAction, joinGameAction;

    private Dictionary<PlayerRef, NetworkObject> _players = new Dictionary<PlayerRef, NetworkObject>();

    private void Awake()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        lookAction = InputSystem.actions.FindAction("Look");
        jumpAction = InputSystem.actions.FindAction("Jump");
        hostGameAction = InputSystem.actions.FindAction("Host");
        joinGameAction = InputSystem.actions.FindAction("Join");
    }

    private void Update()
    {
        if (_runner is not null) return;

        if (hostGameAction.IsPressed()) StartGame(GameMode.Host);
        if (joinGameAction.IsPressed()) StartGame(GameMode.Client);
    }

    private async void StartGame(GameMode mode)
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

        // Start or join (depends on game mode) a game session
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "TestRoom",
            Scene = scene, // This only matters if the game is started by a host - clients must use host's scene
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
        });

        Destroy(_camera.gameObject);
    }

    private void OnGUI()
    {
        // Create a simple placeholder ui for hosting/joining game
        if (_runner is null)
        {
            if (GUI.Button(new Rect(0, 0, 200, 40), "Host")) StartGame(GameMode.Host);
            if (GUI.Button(new Rect(0, 40, 200, 40), "Join")) StartGame(GameMode.Client);
        }
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
        }
    }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
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

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        NetworkInputData data = new NetworkInputData();

        data.direction = moveAction.ReadValue<Vector2>();
        data.look = lookAction.ReadValue<Vector2>();

        data.buttons.Set((int)InputButtons.Jump, jumpAction.IsPressed());

        input.Set(data);
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
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
}
