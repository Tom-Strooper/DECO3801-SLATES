using Fusion;
using Slates.Player;
using Slates.PuzzleInteractions.Physics;
using Slates.PuzzleInteractions.Selection;
using UnityEngine;

namespace Slates.PuzzleInteractions.Controllers
{
    [RequireComponent(typeof(Rigidbody))]
    public class MoveableObjectController : PhysicsInteractorComponent, ISelectable
    {
        [Networked] public bool IsSelected { get; private set; }

        public Rigidbody RB => _rb;
        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public override void Spawned()
        {
            IsSelected = false;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.StateAuthority | RpcTargets.InputAuthority)]
        public void RPC_OnSelected(PlayerController player)
        {
            if (!Runner.IsServer) return;

            IsSelected = true;
            player.RPC_Grab(this);
        }
        [Rpc(RpcSources.StateAuthority, RpcTargets.StateAuthority | RpcTargets.InputAuthority)]
        public void RPC_OnDeselected(PlayerController player)
        {
            if (!Runner.IsServer) return;

            IsSelected = false;
            player.RPC_Drop(this);
        }
    }
}
