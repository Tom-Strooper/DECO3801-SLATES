using Fusion;
using Slates.PuzzleInteractions.Physics;
using UnityEngine;

namespace Slates.PuzzleInteractions
{
    [RequireComponent(typeof(Collider))]
    public class TriggerVolume : NetworkBehaviour
    {
        public TriggerEvent Entered { get; set; } = (_) => { };
        public TriggerEvent Exited { get; set; } = (_) => { };

        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.attachedRigidbody?.GetComponent<PhysicsInteractorComponent>() is null) return;
            Entered(other);
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.attachedRigidbody?.GetComponent<PhysicsInteractorComponent>() is null) return;
            Exited(other);
        }
    }

    public delegate void TriggerEvent(Collider other);
}
