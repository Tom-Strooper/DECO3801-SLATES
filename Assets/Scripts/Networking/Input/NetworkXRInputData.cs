using Fusion;
using UnityEngine;

namespace Slates.Networking.Input
{
    public enum XRInputButtons
    {
        Summon,
        Grab,
        Pinch,
    }

    public struct NetworkXRInputData : INetworkInput
    {
        public NetworkButtons buttons;

        public Vector2 look;

        public Vector3 interactionOrigin;
        public Vector3 interactionDirection;
    }
}