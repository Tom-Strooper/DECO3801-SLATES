using UnityEngine;
using Slates.PuzzleInteractions;
using Slates.PuzzleInteractions.Physics;
using Fusion;

public class PressurePlate : NetworkBehaviour
{
    [Header("Debug Info")]
    [SerializeField] private bool debugMode = true;

    private void Awake()
    {
        // Hook into TriggerVolume events
        TriggerVolume trigger = GetComponentInChildren<TriggerVolume>();
        if (trigger != null)
        {
            trigger.Entered += OnTriggerVolumeEntered;
            trigger.Exited += OnTriggerVolumeExited;

            if (debugMode)
                Debug.Log($"PressurePlate '{name}' hooked into TriggerVolume events");
        }
        else
        {
            Debug.LogError($"PressurePlate '{name}' has no TriggerVolume attached!");
        }
    }

    private void OnTriggerVolumeEntered(Collider other)
    {
        if (debugMode)
            Debug.Log($"TriggerVolume entered by: {other.name} with tag: {other.tag}");

        if (ShouldTriggerPlate(other))
        {
            if (Object.HasStateAuthority) // Only server (or state authority) fires the RPC
            {
                RPC_PlayPlateSound();
            }
        }
    }

    private void OnTriggerVolumeExited(Collider other)
    {
        if (debugMode)
            Debug.Log($"TriggerVolume exited by: {other.name}");
    }

    private bool ShouldTriggerPlate(Collider other)
    {
        // Detect Fusion PhysicsInteractor objects
        if (other.attachedRigidbody?.GetComponent<PhysicsInteractorComponent>() != null)
        {
            if (debugMode)
                Debug.Log($"PhysicsInteractorComponent detected on {other.name}");
            return true;
        }

        // Detect player roots
        NetworkObject playerRoot = other.GetComponentInParent<NetworkObject>();
        if (playerRoot != null && playerRoot.HasInputAuthority)
        {
            if (debugMode)
                Debug.Log($"NetworkObject with input authority detected: {playerRoot.name}");
            return true;
        }

        return false;
    }

    // 🔊 This RPC is broadcast to everyone
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayPlateSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlateSound();
            Debug.Log("Pressure plate sound played universally");
        }
        else
        {
            Debug.LogError("AudioManager.Instance is NULL!");
        }
    }
}
