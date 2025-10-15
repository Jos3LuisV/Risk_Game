using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Configuración de Audio")]
    public AudioSource musicSource;
    public AudioClip musicClip;

    [Header("Ajustes")]
    public bool musicEnabled = true;
    public float musicVolume = 0.7f;

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
            return;
        }

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }

        musicSource.loop = true;
        musicSource.clip = musicClip;
        musicSource.volume = musicEnabled ? musicVolume : 0f;

        CargarAjustes();

        if (musicEnabled && musicClip != null)
        {
            musicSource.Play();
        }
    }

    public void ToggleMusic()
    {
        musicEnabled = !musicEnabled;

        if (musicEnabled)
        {
            musicSource.volume = musicVolume;
            if (!musicSource.isPlaying && musicClip != null)
            {
                musicSource.Play();
            }
        }
        else
        {
            musicSource.volume = 0;
        }

        GuardarAjustes();
        Debug.Log($"🎵 Música {(musicEnabled ? "Activada" : "Desactivada")}");
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicEnabled)
        {
            musicSource.volume = musicVolume;
        }
        GuardarAjustes();
    }

    private void GuardarAjustes()
    {
        PlayerPrefs.SetInt("MusicEnabled", musicEnabled ? 1 : 0);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.Save();
    }

    private void CargarAjustes()
    {
        musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);

        if (musicSource != null)
        {
            musicSource.volume = musicEnabled ? musicVolume : 0;
        }
    }
}
