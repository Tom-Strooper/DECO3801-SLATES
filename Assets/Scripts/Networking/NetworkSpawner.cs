using Fusion;
using Slates.Utility;
using UnityEngine;

namespace Slates.Networking
{
    public class NetworkSpawner : NetworkBehaviour, IPlayerJoined, IPlayerLeft
    {
        [Networked, Capacity(Constants.MaxPlayers)] private NetworkDictionary<PlayerRef, NetworkObject> _players { get; } = new NetworkDictionary<PlayerRef, NetworkObject>();

        public Transform VRSpawn => _vrPlayerSpawn;
        public Transform NonVRSpawn => _nonVrPlayerSpawnOrigin;

        public BackgroundInfo Info { get; private set; }

        [Header("VR Player Settings")]
        [SerializeField] private NetworkPrefabRef _vrPlayerPrefab;
        [SerializeField] private Transform _vrPlayerSpawn;

        [Header("Non-VR Player Settings")]
        [SerializeField] private NetworkPrefabRef _nonVrPlayerPrefab;
        [SerializeField] private Transform _nonVrPlayerSpawnOrigin;
        [SerializeField] private float _nonVrPlayerSpawnRadius = 4.0f;

        public void PlayerJoined(PlayerRef player)
        {
            // Only the server should spawn the players
            if (!HasStateAuthority) return;

            // Spawn player (first player to join is VR player)
            // TODO - This probably needs to be cleverer at detecting host/client
            NetworkObject playerObject = _players.Count == 0 ? SpawnVrPlayer(player) : SpawnNonVrPlayer(player);
            _players.Add(player, playerObject);
        }

        public void PlayerLeft(PlayerRef player)
        {
            // Only the server should despawn players
            if (!HasStateAuthority || !_players.ContainsKey(player)) return;

            // Remove & despawn the player
            Runner.Despawn(_players[player]);
            _players.Remove(player);
        }

        private NetworkObject SpawnVrPlayer(PlayerRef player)
        {
            Debug.Log("Spawning VR player");
            return Runner.Spawn(_vrPlayerPrefab, _vrPlayerSpawn.position, _vrPlayerSpawn.rotation, player);
        }
        private NetworkObject SpawnNonVrPlayer(PlayerRef player)
        {
            Debug.Log("Spawning Non-VR player");

            float angle = 2.0f * Mathf.PI * (player.AsIndex - 1) / (Constants.MaxPlayers - 1);

            Vector3 pos = _nonVrPlayerSpawnOrigin.position + Vector3.up + _nonVrPlayerSpawnRadius * new Vector3(Mathf.Cos(angle), 0.0f, Mathf.Sin(angle));
            Quaternion rot = _nonVrPlayerSpawnOrigin.rotation * Quaternion.Euler(Vector3.up * angle);

            return Runner.Spawn(_nonVrPlayerPrefab, pos, rot, player);
        }
    }
}