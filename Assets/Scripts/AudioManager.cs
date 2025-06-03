using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{

    [Header("Music Settings")]
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.25f;
    [SerializeField] private AudioSource musicSource;

    [SerializeField] private AudioClip mainTheme;
    [SerializeField] private AudioClip combatTheme;
    [SerializeField] private AudioClip victoryTheme;
    [SerializeField] private AudioClip defeatTheme;

    [Header("Sound Effects Settings")]
    [SerializeField, Range(0f, 1f)] private float soundEffectsVolume = 1f;
    [SerializeField] private GameObject soundEffectPrefab;

    void Start()
    {
        musicSource.volume = musicVolume;
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

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
    }

    public float GetMusicVolume()
    {
        return musicSource.volume;
    }

    public void SetSfxVolume(float volume)
    {
        soundEffectsVolume = Mathf.Clamp01(volume);
    }

    public float GetSfxVolume()
    {
        return soundEffectsVolume;
    }
    public void PlaySoundEffect(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioManager: Tried to play a null AudioClip for a 2D sound effect.");
            return;
        }

        AudioSource source = GetConfiguredAudioSource(clip, transform.position, soundEffectsVolume, 0.0f); // 0.0f for full 2D
        source.Play();
        Destroy(source.gameObject, clip.length / source.pitch);
    }

    private AudioSource GetConfiguredAudioSource(AudioClip clip, Vector3 position, float volume, float spatialBlend)
    {
        GameObject soundGameObject;
        AudioSource audioSource;

        if (soundEffectPrefab != null)
        {
            soundGameObject = Instantiate(soundEffectPrefab, position, Quaternion.identity);
            audioSource = soundGameObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                // This is a fallback, ideally the prefab should be correctly configured.
                Debug.LogWarning("AudioManager: SoundEffectPrefab is missing an AudioSource component. Adding one dynamically.");
                audioSource = soundGameObject.AddComponent<AudioSource>();
            }
        }
        else
        {
            soundGameObject = new GameObject($"OneShotAudio_{clip.name}");
            soundGameObject.transform.position = position;
            audioSource = soundGameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.spatialBlend = spatialBlend;
        audioSource.playOnAwake = false; // We will call Play() manually

        // You might want to configure other AudioSource properties here or on the prefab,
        // e.g., outputAudioMixerGroup, minDistance, maxDistance, rolloffMode.

        return audioSource;
    }
}
