using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Music Sources")]
    public AudioSource musicSource;

    [Header("Sound Effects")]
    public AudioSource sfxSource;

    [Header("Music Clips")]
    public AudioClip mainMenuMusic;
    public AudioClip[] levelMusic; 

    [Header("Sound Effects Clips")]
    public AudioClip hitSFX;
    public AudioClip buttonClickSFX;

    void Awake()
    {
        // Singleton Pattern
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

    private void Start()
    {
       
    }

    

    public void PlayMainMenuMusic()
    {
        if (musicSource.clip != mainMenuMusic)
        {
            musicSource.clip = mainMenuMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlayLevelMusic(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < levelMusic.Length)
        {
            if (musicSource.clip != levelMusic[levelIndex])
            {
                musicSource.clip = levelMusic[levelIndex];
                musicSource.loop = true;
                musicSource.Play();
            }
        }
        else
        {
            Debug.LogWarning("Level music index out of range.");
        }
    }

    public void PlayHitSFX()
    {
        sfxSource.PlayOneShot(hitSFX);
    }

    public void PlayButtonClickSFX()
    {
        sfxSource.PlayOneShot(buttonClickSFX);
    }
}
