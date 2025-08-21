using Fusion;
using UnityEngine;

public enum InputButtons
{
    Jump,
}

public struct NetworkInputData : INetworkInput
{
    public Vector2 direction;
    public Vector2 look;
    public NetworkButtons buttons;
}
