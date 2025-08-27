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

        private bool _active;
        private bool _enabled = true;

        [Header("Visuals")]
        [SerializeField] private MeshRenderer _renderer;
        [SerializeField] private Material _inactiveMaterial;
        [SerializeField] private Material _activeMaterial;

        private void Update()
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

        public void Reset() => Deactivate();
        public void Disable() => _enabled = false;

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
