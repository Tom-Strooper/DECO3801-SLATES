using UnityEngine;
using Slates.PuzzleInteractions;
using Slates.PuzzleInteractions.Physics;
using Fusion;

public class PressurePlate : NetworkBehaviour
{
    [Header("Debug Info")]
    [SerializeField] private bool debugMode = true;

    //Tracks activation
    [Networked] private bool HasActivated { get; set; } = false;

    private bool localActivated = false;
    private void Awake()
    {
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
            if (localActivated) return;
    
            if (Object.HasStateAuthority && !HasActivated)
            {
                HasActivated = true;
                localActivated = true;
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
        if (other.attachedRigidbody?.GetComponent<PhysicsInteractorComponent>() != null)
        {
            if (debugMode)
                Debug.Log($"PhysicsInteractorComponent detected on {other.name}");
            return true;
        }

        NetworkObject playerRoot = other.GetComponentInParent<NetworkObject>();
        if (playerRoot != null && playerRoot.HasInputAuthority)
        {
            if (debugMode)
                Debug.Log($"NetworkObject with input authority detected: {playerRoot.name}");
            return true;
        }

        return false;
    }

    // Broadcast to everyone
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayPlateSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlateSound();
            Debug.Log("Pressure plate sound played universally (first activation)");
        }
        else
        {
            Debug.LogError("AudioManager.Instance is NULL!");
        }
    }
}
