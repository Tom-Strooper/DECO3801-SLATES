using Slates.Player;
using UnityEngine;

namespace Slates.PuzzleInteractions.Selection
{
    public interface ISelectable
    {
        public bool IsSelected { get; }
        public Rigidbody RB { get; }

        public void OnSelected(PlayerController player);
        public void OnDeselected(PlayerController player);
    }
}
