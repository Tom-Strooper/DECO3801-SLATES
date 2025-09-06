using Fusion;
using Slates.PuzzleInteractions.Physics;
using UnityEngine;

namespace Slates.PuzzleInteractions
{
    [RequireComponent(typeof(Collider))]
    public class TriggerVolume : NetworkBehaviour
    {
        public bool Occupied => _nOccupants > 0;
        [Networked] private int _nOccupants { get; set; }

        public TriggerEvent Entered { get; set; } = (_) => { };
        public TriggerEvent Exited { get; set; } = (_) => { };

        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        public override void Spawned()
        {
            _nOccupants = 0;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (Runner.IsServer && other.attachedRigidbody?.GetComponent<PhysicsInteractorComponent>() is null) return;

            Entered(other);
            RPC_IncOccupants();
        }
        private void OnTriggerExit(Collider other)
        {
            if (Runner.IsServer && other.attachedRigidbody?.GetComponent<PhysicsInteractorComponent>() is null) return;

            Exited(other);
            RPC_DecOccupants();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.StateAuthority | RpcTargets.InputAuthority)]
        private void RPC_IncOccupants() => _nOccupants++;
        [Rpc(RpcSources.StateAuthority, RpcTargets.StateAuthority | RpcTargets.InputAuthority)]
        private void RPC_DecOccupants() => _nOccupants--;
    }

    public delegate void TriggerEvent(Collider other);
}
