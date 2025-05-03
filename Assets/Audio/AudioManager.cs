using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource musicSource;
    public AudioClip mainTheme;
    public AudioClip combatTheme;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        PlayMainTheme();
    }

    public void PlayMainTheme()
    {
        musicSource.clip = mainTheme;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayCombatTheme()
    {
        musicSource.clip = combatTheme;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void SetVolume(float volume)
    {
        musicSource.volume = volume;
    }
}
