using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    private AudioManager audioManager;

    private void Start()
    {
        // Use the new recommended method
        audioManager = AudioManager.FindFirstObjectByType<AudioManager>();
        
        // Optional safety check
        if (audioManager == null)
        {
            Debug.LogError("AudioManager not found in scene!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Block"))
        {
            audioManager.PlaySFX(audioManager.plate);
        }
    }
}
