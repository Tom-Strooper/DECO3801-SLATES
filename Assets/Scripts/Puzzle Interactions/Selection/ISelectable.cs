using Slates.Player;
using UnityEngine;

namespace Slates.PuzzleInteractions.Selection
{
    public interface ISelectable
    {
        public bool IsSelected { get; }
        public Rigidbody RB { get; }

        public void RPC_OnSelected(PlayerController player);
        public void RPC_OnDeselected(PlayerController player);
    }
}
