using Fusion;
using UnityEngine;

namespace Slates.PuzzleInteractions.Controllers
{
    public class PressurePlateController : NetworkBehaviour, IPuzzleInteractor
    {
        public PuzzleInteractionController Owner { get; private set; }

        public string Key => _key;
        [SerializeField] private string _key;

        [SerializeField] private InteractorActivationMode _mode;
        [SerializeField] private TriggerVolume _activationVolume;

        [Networked] private bool _active { get; set; }
        [Networked] private bool _enabled { get; set; }

        [Header("Visuals")]
        [SerializeField] private MeshRenderer _renderer;
        [SerializeField] private Material _inactiveMaterial;
        [SerializeField] private Material _activeMaterial;

        public override void Spawned()
        {
            base.Spawned();

            _active = false;
            _enabled = true;
        }

        public override void Render()
        {
            _renderer.material = _active ? _activeMaterial : _inactiveMaterial;
        }

        public void Init(PuzzleInteractionController owner)
        {
            Owner = owner;

            _activationVolume.Entered += (_) => Activate();
            switch (_mode)
            {
                case InteractorActivationMode.Hold:
                    _activationVolume.Exited += (_) => Deactivate();
                    break;
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority | RpcTargets.StateAuthority)]
        public void RPC_Reset() => Deactivate();
        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority | RpcTargets.StateAuthority)]
        public void RPC_Disable() => _enabled = false;

        private void Activate()
        {
            if (!_enabled) return;

            _active = true;
            Owner.Activate(Key);
        }
        private void Deactivate()
        {
            if (!_enabled) return;

            _active = false;
            Owner.Deactivate(Key);
        }
    }
}
