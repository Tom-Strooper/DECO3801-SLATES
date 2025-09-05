using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("-------- Audio Source --------")]
    [SerializeField] AudioSource backgroundSource;
    [SerializeField] AudioSource SFXSource;

    [Header("-------- Audio Clip --------")]
    public AudioClip background;
    public AudioClip plate;
    public AudioClip puzzleComplete;
    public AudioClip blockPickup;

    private void Start()
    {
        backgroundSource.clip = background;
        backgroundSource.loop = true; // make background loop
        backgroundSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            SFXSource.PlayOneShot(clip);
        }
    }
}
