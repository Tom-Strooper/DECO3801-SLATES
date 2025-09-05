using UnityEngine;
using UnityEngine.InputSystem;

public class AudioManager : MonoBehaviour
{
    [Header("-------- Audio Sources --------")]
    [SerializeField] private AudioSource backgroundSource;
    [SerializeField] private AudioSource SFXSource;

    [Header("-------- Audio Clips --------")]
    public AudioClip background;
    public AudioClip plate;
    public AudioClip puzzleComplete;
    public AudioClip blockPickup;

    // Singleton-like access (optional)
    public static AudioManager Instance;

    private void Awake()
    {
        // Make this easily accessible by other scripts
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: persist across scenes
            Debug.Log("AudioManager Instance created");
        }
        else
        {
            Debug.Log("Duplicate AudioManager destroyed");
            Destroy(gameObject); // prevent duplicates
        }
    }

    private void Start()
    {
        // Validate audio sources and clips
        ValidateAudioSetup();
        
        // Play background music on loop
        if (backgroundSource != null && background != null)
        {
            backgroundSource.clip = background;
            backgroundSource.loop = true;
            backgroundSource.Play();
            Debug.Log("Background music started");
        }
    }

    private void ValidateAudioSetup()
    {
        Debug.Log("=== Audio Manager Setup Validation ===");
        Debug.Log($"Background Source: {(backgroundSource != null ? "Valid" : "NULL")}");
        Debug.Log($"SFX Source: {(SFXSource != null ? "Valid" : "NULL")}");
        Debug.Log($"Background Clip: {(background != null ? background.name : "NULL")}");
        Debug.Log($"Plate Clip: {(plate != null ? plate.name : "NULL")}");
        Debug.Log($"Puzzle Complete Clip: {(puzzleComplete != null ? puzzleComplete.name : "NULL")}");
        Debug.Log($"Block Pickup Clip: {(blockPickup != null ? blockPickup.name : "NULL")}");
        
        if (SFXSource != null)
        {
            Debug.Log($"SFX Source Volume: {SFXSource.volume}");
            Debug.Log($"SFX Source Mute: {SFXSource.mute}");
            Debug.Log($"SFX Source Enabled: {SFXSource.enabled}");
        }
    }

    private void Update()
    {
        // Debug: play plate sound via P key
        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            PlaySFX(plate);
            Debug.Log("Plate sound played via P key");
        }
    }

    /// <summary>
    /// Play any SFX clip through the SFX audio source
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (SFXSource != null && clip != null)
        {
            Debug.Log($"Playing SFX: {clip.name}");
            SFXSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"Cannot play SFX. SFXSource: {(SFXSource != null ? "Valid" : "NULL")}, clip: {(clip != null ? clip.name : "NULL")}");
        }
    }

    /// <summary>
    /// Convenience function specifically for pressure plate sound
    /// </summary>
    public void PlayPlateSound()
    {
        Debug.Log("PlayPlateSound called");
        PlaySFX(plate);
    }
}