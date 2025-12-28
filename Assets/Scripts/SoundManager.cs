using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    
    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1f;
        [Range(0.1f, 3f)]
        public float pitch = 1f;
        public bool loop = false;
        
        [HideInInspector]
        public AudioSource source;
    }
    
    [Header("Sound Effects")]
    public Sound[] soundEffects;
    
    [Header("Music")]
    public Sound[] musicTracks;
    
    [Header("Settings")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;
    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    [Range(0f, 1f)]
    public float musicVolume = 1f;
    
    private Dictionary<string, Sound> soundDictionary = new Dictionary<string, Sound>();
    private Dictionary<string, Sound> musicDictionary = new Dictionary<string, Sound>();
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSounds();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeSounds()
    {
        // Initialize sound effects
        foreach (Sound sound in soundEffects)
        {
            if (sound.clip == null)
            {
                Debug.LogWarning($"Sound '{sound.name}' has no AudioClip assigned!");
                continue;
            }
            
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume * sfxVolume * masterVolume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;
            
            soundDictionary[sound.name] = sound;
        }
        
        // Initialize music tracks
        foreach (Sound music in musicTracks)
        {
            if (music.clip == null)
            {
                Debug.LogWarning($"Music '{music.name}' has no AudioClip assigned!");
                continue;
            }
            
            music.source = gameObject.AddComponent<AudioSource>();
            music.source.clip = music.clip;
            music.source.volume = music.volume * musicVolume * masterVolume;
            music.source.pitch = music.pitch;
            music.source.loop = music.loop;
            
            musicDictionary[music.name] = music;
        }
    }
    
    // Play sound effect
    public void PlaySound(string soundName)
    {
        if (soundDictionary.TryGetValue(soundName, out Sound sound))
        {
            if (sound.source != null)
            {
                sound.source.volume = sound.volume * sfxVolume * masterVolume;
                sound.source.Play();
            }
        }
        else
        {
            Debug.LogWarning($"Sound '{soundName}' not found!");
        }
    }
    
    // Play sound effect at position (3D sound)
    public void PlaySoundAtPosition(string soundName, Vector3 position)
    {
        if (soundDictionary.TryGetValue(soundName, out Sound sound))
        {
            if (sound.clip != null)
            {
                AudioSource.PlayClipAtPoint(sound.clip, position, sound.volume * sfxVolume * masterVolume);
            }
        }
        else
        {
            Debug.LogWarning($"Sound '{soundName}' not found!");
        }
    }
    
    // Play sound with random pitch variation
    public void PlaySoundWithRandomPitch(string soundName, float pitchVariation = 0.1f)
    {
        if (soundDictionary.TryGetValue(soundName, out Sound sound))
        {
            if (sound.source != null)
            {
                sound.source.volume = sound.volume * sfxVolume * masterVolume;
                sound.source.pitch = sound.pitch + Random.Range(-pitchVariation, pitchVariation);
                sound.source.Play();
            }
        }
        else
        {
            Debug.LogWarning($"Sound '{soundName}' not found!");
        }
    }
    
    // Stop sound effect
    public void StopSound(string soundName)
    {
        if (soundDictionary.TryGetValue(soundName, out Sound sound))
        {
            if (sound.source != null && sound.source.isPlaying)
            {
                sound.source.Stop();
            }
        }
    }
    
    // Play music
    public void PlayMusic(string musicName)
    {
        if (musicDictionary.TryGetValue(musicName, out Sound music))
        {
            if (music.source != null)
            {
                music.source.volume = music.volume * musicVolume * masterVolume;
                music.source.Play();
            }
        }
        else
        {
            Debug.LogWarning($"Music '{musicName}' not found!");
        }
    }
    
    // Stop music
    public void StopMusic(string musicName)
    {
        if (musicDictionary.TryGetValue(musicName, out Sound music))
        {
            if (music.source != null && music.source.isPlaying)
            {
                music.source.Stop();
            }
        }
    }
    
    // Stop all music
    public void StopAllMusic()
    {
        foreach (var music in musicDictionary.Values)
        {
            if (music.source != null && music.source.isPlaying)
            {
                music.source.Stop();
            }
        }
    }
    
    // Fade in music
    public void FadeInMusic(string musicName, float duration = 1f)
    {
        if (musicDictionary.TryGetValue(musicName, out Sound music))
        {
            if (music.source != null)
            {
                StartCoroutine(FadeInCoroutine(music.source, music.volume * musicVolume * masterVolume, duration));
            }
        }
    }
    
    // Fade out music
    public void FadeOutMusic(string musicName, float duration = 1f)
    {
        if (musicDictionary.TryGetValue(musicName, out Sound music))
        {
            if (music.source != null)
            {
                StartCoroutine(FadeOutCoroutine(music.source, duration));
            }
        }
    }
    
    private System.Collections.IEnumerator FadeInCoroutine(AudioSource source, float targetVolume, float duration)
    {
        source.volume = 0f;
        source.Play();
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
            yield return null;
        }
        
        source.volume = targetVolume;
    }
    
    private System.Collections.IEnumerator FadeOutCoroutine(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }
        
        source.volume = 0f;
        source.Stop();
    }
    
    // Update volume settings
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateSFXVolumes();
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateMusicVolumes();
    }
    
    private void UpdateAllVolumes()
    {
        UpdateSFXVolumes();
        UpdateMusicVolumes();
    }
    
    private void UpdateSFXVolumes()
    {
        foreach (var sound in soundDictionary.Values)
        {
            if (sound.source != null)
            {
                sound.source.volume = sound.volume * sfxVolume * masterVolume;
            }
        }
    }
    
    private void UpdateMusicVolumes()
    {
        foreach (var music in musicDictionary.Values)
        {
            if (music.source != null)
            {
                music.source.volume = music.volume * musicVolume * masterVolume;
            }
        }
    }
    
    // Check if sound is playing
    public bool IsSoundPlaying(string soundName)
    {
        if (soundDictionary.TryGetValue(soundName, out Sound sound))
        {
            return sound.source != null && sound.source.isPlaying;
        }
        return false;
    }
    
    // Check if music is playing
    public bool IsMusicPlaying(string musicName)
    {
        if (musicDictionary.TryGetValue(musicName, out Sound music))
        {
            return music.source != null && music.source.isPlaying;
        }
        return false;
    }
}
