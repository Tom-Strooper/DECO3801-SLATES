using Fusion;
using UnityEngine;

namespace Slates.Networking.Input
{
    public enum InputButtons
    {
        Jump,
        Select,
        Interact
    }

    public struct NetworkInputData : INetworkInput
    {
        public Vector2 direction;
        public Vector2 look;
        public NetworkButtons buttons;
    }

}
