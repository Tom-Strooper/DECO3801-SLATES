using Fusion;
using UnityEngine;

namespace Slates.PuzzleInteractions.Components
{
    [RequireComponent(typeof(NetworkObject))]
    public class DoorController : NetworkBehaviour, IInteractionReceiver
    {
        public PuzzleInteractionController Owner { get; private set; }

        private NetworkObject _obj;

        public void Init(PuzzleInteractionController owner)
        {
            Owner = owner;
        }

        public override void Spawned()
        {
            _obj = GetComponent<NetworkObject>();
        }

        public void OnInteractionReceived()
        {
            // Can probably play some sort of animation here instead of just flat-out destroying it
            Runner.Despawn(_obj);
        }
    }
}
