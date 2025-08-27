using Slates.Player;
using Slates.PuzzleInteractions.Physics;
using Slates.PuzzleInteractions.Selection;
using UnityEngine;

namespace Slates.PuzzleInteractions.Controllers
{
    [RequireComponent(typeof(Rigidbody))]
    public class MoveableObjectController : PhysicsInteractorComponent, ISelectable
    {
        public void OnSelected(PlayerController player)
        {
            player.Grab(GetComponent<Rigidbody>());
        }
    }
}
