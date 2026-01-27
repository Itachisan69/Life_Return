using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip musicClip;
    [Range(0f, 1f)] public float volume = 0.5f;
    public bool playOnAwake = true;
    public bool loop = true;

    private AudioSource audioSource;
    private static BackgroundMusic instance;

    private void Awake()
    {
        // Singleton Pattern: Prevents multiple music objects from existing at once
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Keeps music playing across scenes
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Setup the AudioSource component
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = musicClip;
        audioSource.volume = volume;
        audioSource.loop = loop;
        audioSource.playOnAwake = playOnAwake;

        if (playOnAwake)
        {
            audioSource.Play();
        }
    }

    // Call this from other scripts if you want to change the volume mid-game
    public void SetVolume(float newVolume)
    {
        volume = newVolume;
        if (audioSource != null) audioSource.volume = volume;
    }
}