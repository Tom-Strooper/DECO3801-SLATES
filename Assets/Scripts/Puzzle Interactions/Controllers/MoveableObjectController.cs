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
        [Networked] public bool IsSelected { get; private set; } = false;

        public Rigidbody RB => _rb;
        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public void OnSelected(PlayerController player)
        {
            IsSelected = true;
            player.Grab(this);
        }
        public void OnDeselected(PlayerController player)
        {
            IsSelected = false;
            player.Drop(this);
        }
    }
}
