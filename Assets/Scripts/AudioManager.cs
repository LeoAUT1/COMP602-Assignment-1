using System.Collections;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{

    [Header("Music Settings")]
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.25f;
    [SerializeField] private AudioSource musicSource;

    [SerializeField] private AudioClip menuTheme;
    [SerializeField] private AudioClip mainTheme;
    [SerializeField] private AudioClip combatTheme;
    [SerializeField] private AudioClip finalBossTheme;
    [SerializeField] private AudioClip victoryTheme;
    [SerializeField] private AudioClip defeatTheme;

    [Header("Sound Effects Settings")]
    [SerializeField, Range(0f, 1f)] private float soundEffectsVolume = 1f;
    [SerializeField] private GameObject soundEffectPrefab;

    private Coroutine activeMusicFadeCoroutine = null;

    void Start()
    {
        musicSource.volume = musicVolume;
    }

    // Helper to stop and clear any active music fade coroutine
    private void StopActiveFade()
    {
        if (activeMusicFadeCoroutine != null)
        {
            StopCoroutine(activeMusicFadeCoroutine);
            activeMusicFadeCoroutine = null;
        }
    }

    // Private helper method to play music tracks
    private void PlayMusicTrack(AudioClip clipToPlay, bool shouldLoop)
    {
        if (musicSource == null)
        {
            Debug.LogError("AudioManager: MusicSource is not assigned!");
            return;
        }

        StopActiveFade(); // Stop any ongoing fade before playing new music

        if (clipToPlay == null)
        {
            Debug.LogWarning($"AudioManager: Attempted to play a null music AudioClip. Stopping current music.");
            musicSource.Stop();
            musicSource.clip = null; // Clear the clip
            return;
        }

        musicSource.clip = clipToPlay;
        musicSource.loop = shouldLoop;
        musicSource.volume = musicVolume; // Set to the master volume setting
        musicSource.Play();
    }

    public void PlayMenuTheme()
    {
        PlayMusicTrack(menuTheme, true);
    }

    public void PlayMainTheme()
    {
        PlayMusicTrack(mainTheme, true);
    }

    public void PlayCombatTheme()
    {
        PlayMusicTrack(combatTheme, true);
    }

    public void PlayFinalBossTheme()
    {
        PlayMusicTrack(finalBossTheme, true);
    }

    public void PlayVictoryTheme()
    {
        PlayMusicTrack(victoryTheme, false);
    }

    public void PlayDefeatTheme()
    {
        PlayMusicTrack(defeatTheme, false);
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
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

    public Coroutine FadeOutMusic(float duration)
    {
        if (musicSource == null)
        {
            Debug.LogError("AudioManager: MusicSource is not assigned. Cannot fade out music.");
            return null;
        }

        StopActiveFade(); // Stop any previous fade before starting a new one

        activeMusicFadeCoroutine = StartCoroutine(FadeOutMusicCoroutine(duration));
        return activeMusicFadeCoroutine;
    }

    private IEnumerator FadeOutMusicCoroutine(float duration)
    {
        float startVolume = musicSource.volume; // Current actual volume of the music source
        float elapsedTime = 0f;

        // Handle invalid or zero duration by setting volume to 0 immediately
        if (duration <= 0f)
        {
            musicSource.volume = 0f;
            musicSource.Stop(); // Stop playback as it's silent
            activeMusicFadeCoroutine = null; // Clear the reference
            yield break; // Exit the coroutine
        }

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / duration);
            yield return null; // Wait for the next frame
        }

        musicSource.volume = 0f; // Ensure it's exactly zero at the end
        musicSource.Stop(); // Stop playback once faded out
        activeMusicFadeCoroutine = null; // Clear the reference as the coroutine has completed
    }
}
