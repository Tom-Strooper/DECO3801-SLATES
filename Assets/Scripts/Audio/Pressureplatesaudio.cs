using UnityEngine;
using Slates.PuzzleInteractions;
using Slates.PuzzleInteractions.Physics;
using Fusion;

public class PressurePlate : MonoBehaviour
{
    [Header("Debug Info")]
    [SerializeField] private bool debugMode = true;

    private void Awake()
    {
        // Try to find a TriggerVolume on this object or its children
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
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPlateSound();
                Debug.Log("Pressure plate triggered by " + other.name + " - Sound should play");
            }
            else
            {
                Debug.LogError("AudioManager.Instance is NULL!");
            }
        }
        else
        {
            if (debugMode)
                Debug.Log($"Object '{other.name}' did not meet trigger conditions");
        }
    }

    private void OnTriggerVolumeExited(Collider other)
    {
        if (debugMode)
            Debug.Log($"TriggerVolume exited by: {other.name}");
    }

    private bool ShouldTriggerPlate(Collider other)
    {
        // 1️⃣ Check if it has a PhysicsInteractorComponent (common in Fusion KCC colliders)
        if (other.attachedRigidbody?.GetComponent<PhysicsInteractorComponent>() != null)
        {
            if (debugMode)
                Debug.Log($"PhysicsInteractorComponent detected on {other.name}");
            return true;
        }

        // 2️⃣ Optional: Check if the parent has a NetworkObject with input authority
        NetworkObject playerRoot = other.GetComponentInParent<NetworkObject>();
        if (playerRoot != null && playerRoot.HasInputAuthority)
        {
            if (debugMode)
                Debug.Log($"NetworkObject with input authority detected: {playerRoot.name}");
            return true;
        }

        return false; // Ignore anything else
    }
}
