using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music Settings")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private bool playOnAwake = true;
    [SerializeField] private bool loop = true;
    [Range(0f, 1f)] [SerializeField] private float volume = 0.5f;

    private AudioSource musicSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = backgroundMusic;
        musicSource.loop = loop;
        musicSource.volume = volume;
        musicSource.playOnAwake = playOnAwake;

        if (playOnAwake && backgroundMusic != null)
        {
            musicSource.Play();
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource.clip == clip) return;
        
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }
}