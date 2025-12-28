using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Combat Sounds")]
    public AudioClip[] shootSounds;
    public AudioClip[] reloadSounds;
    public AudioClip[] hitSounds;
    public AudioClip[] footstepSounds;
    
    [Header("UI Sounds")]
    public AudioClip buttonClickSound;
    public AudioClip gameStartSound;
    public AudioClip gameOverSound;
    public AudioClip roundWinSound;
    public AudioClip roundLoseSound;
    
    [Header("Ambient")]
    public AudioClip backgroundMusic;
    public AudioClip[] ambientSounds;
    
    [Header("Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    
    private AudioSource musicSource;
    private AudioSource sfxSource;
    
    public static AudioManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void SetupAudioSources()
    {
        // 음악용 오디오 소스
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = musicVolume * masterVolume;
        
        // 효과음용 오디오 소스
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.volume = sfxVolume * masterVolume;
        
        // 배경음악 재생
        if (backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
    }
    
    public void PlayShootSound()
    {
        PlayRandomSound(shootSounds);
    }
    
    public void PlayReloadSound()
    {
        PlayRandomSound(reloadSounds);
    }
    
    public void PlayHitSound()
    {
        PlayRandomSound(hitSounds);
    }
    
    public void PlayFootstepSound()
    {
        PlayRandomSound(footstepSounds);
    }
    
    public void PlayButtonClick()
    {
        PlaySound(buttonClickSound);
    }
    
    public void PlayGameStart()
    {
        PlaySound(gameStartSound);
    }
    
    public void PlayGameOver()
    {
        PlaySound(gameOverSound);
    }
    
    public void PlayRoundWin()
    {
        PlaySound(roundWinSound);
    }
    
    public void PlayRoundLose()
    {
        PlaySound(roundLoseSound);
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume * masterVolume);
        }
    }
    
    private void PlayRandomSound(AudioClip[] clips)
    {
        if (clips != null && clips.Length > 0)
        {
            AudioClip randomClip = clips[Random.Range(0, clips.Length)];
            PlaySound(randomClip);
        }
    }
    
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }
    
    private void UpdateVolumes()
    {
        if (musicSource != null)
            musicSource.volume = musicVolume * masterVolume;
        
        if (sfxSource != null)
            sfxSource.volume = sfxVolume * masterVolume;
    }
}