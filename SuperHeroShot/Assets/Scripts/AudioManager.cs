using UnityEngine;

namespace rescueforce
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Sources (optional, created at runtime if null)")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Volumes")]
        [Range(0f, 1f)][SerializeField] private float musicVolume = 1f;
        [Range(0f, 1f)][SerializeField] private float sfxVolume = 1f;

        public AudioClip backgroundMusic;
        public AudioClip shootSfx;
        public AudioClip hurtSfx;

        public AudioClip laserSfx;
        public AudioClip rocketSfx;
        public AudioClip targetLockSfx;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.volume = musicVolume;

            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.volume = sfxVolume;
            PlayMusic(backgroundMusic);
        }

        // MUSIC
        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (clip == null) return;
            if (musicSource.clip == clip)
            {
                musicSource.loop = loop;
                if (!musicSource.isPlaying) musicSource.Play();
                return;
            }
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }

        public void StopMusic()
        {
            if (musicSource.isPlaying) musicSource.Stop();
            musicSource.clip = null;
        }

        public void SetMusicVolume(float v)
        {
            musicVolume = Mathf.Clamp01(v);
            if (musicSource != null) musicSource.volume = musicVolume;
        }

        public float GetMusicVolume() => musicVolume;

        // SFX
        public void PlaySfx(AudioClip clip, float volume = 1f)
        {
            if (clip == null || sfxSource == null) return;
            sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume) * sfxVolume);
        }

        public void SetSfxVolume(float v)
        {
            sfxVolume = Mathf.Clamp01(v);
            if (sfxSource != null) sfxSource.volume = sfxVolume;
        }

        public float GetSfxVolume() => sfxVolume;

        public void MuteAll(bool mute)
        {
            if (musicSource != null) musicSource.mute = mute;
            if (sfxSource != null) sfxSource.mute = mute;
        }

        public void PlayShootSfx()
        {
            PlaySfx(shootSfx);
        }

        public void PlayHurtSfx()
        {
            PlaySfx(hurtSfx);
        }

        public void PlayLaserSfx()
        {
            PlaySfx(laserSfx);
        }

        public void PlayRocketSfx()
        {
            PlaySfx(rocketSfx);
        }

        public void PlayTargetLockSfx()
        {
            PlaySfx(targetLockSfx);
        }

        public void SetActiveMusic(bool active)
        {
            if (musicSource == null) return;
            if (active && !musicSource.isPlaying)
                musicSource.Play();
            else if (!active && musicSource.isPlaying)
                {
                    musicSource.Stop();
                    Debug.Log("Music stopped");
                }
            PlayerPrefs.SetInt("MusicActive", active ? 1 : 0);
            PlayerPrefs.Save();
        }


        public void SetActiveSfx(bool active)
        {
            if (sfxSource == null) return;
            sfxSource.mute = !active;
            Debug.Log($"SFX {(active ? "enabled" : "disabled")}");
            PlayerPrefs.SetInt("SfxActive", active ? 1 : 0);
            PlayerPrefs.Save();
        }

        public bool IsMusicActive() => musicSource != null && musicSource.isPlaying;
        public bool IsSfxActive() => sfxSource != null && !sfxSource.mute;

    }

}
