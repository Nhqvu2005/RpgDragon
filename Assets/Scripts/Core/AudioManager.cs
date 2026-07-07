using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGDragon.Core
{
    /// <summary>
    /// Singleton manager for all audio in the game. Provides pool-based SFX
    /// playback, cross-fading BGM, and persistent volume settings saved via
    /// PlayerPrefs.
    ///
    /// <para>
    /// Place one AudioManager prefab in the initial scene. It will survive
    /// scene transitions via DontDestroyOnLoad.
    /// </para>
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private int sfxPoolSize = 5;

        [Header("Volume")]
        [Range(0f, 1f)]
        [SerializeField] private float bgmVolume = 0.5f;
        [Range(0f, 1f)]
        [SerializeField] private float sfxVolume = 0.5f;

        private List<AudioSource> _sfxPool;
        private Coroutine _bgmFadeCoroutine;

        private const string BGM_VOL_KEY = "RPGDragon_BGMVolume";
        private const string SFX_VOL_KEY = "RPGDragon_SFXVolume";

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Persisted volume settings.
            bgmVolume = PlayerPrefs.GetFloat(BGM_VOL_KEY, 0.5f);
            sfxVolume = PlayerPrefs.GetFloat(SFX_VOL_KEY, 0.5f);

            // Create the dedicated BGM source if none was assigned.
            if (bgmSource == null)
            {
                GameObject bgmObj = new GameObject("BGM_Source");
                bgmObj.transform.SetParent(transform);
                bgmSource = bgmObj.AddComponent<AudioSource>();
                bgmSource.loop = true;
                bgmSource.playOnAwake = false;
            }
            bgmSource.volume = bgmVolume;

            // Pre-warm the SFX source pool.
            _sfxPool = new List<AudioSource>(sfxPoolSize);
            for (int i = 0; i < sfxPoolSize; i++)
            {
                CreateSfxSource();
            }
        }

        // ── Pool Helpers ──────────────────────────────────────────────────────

        private AudioSource CreateSfxSource()
        {
            GameObject sfxObj = new GameObject("SFX_Source_" + _sfxPool.Count);
            sfxObj.transform.SetParent(transform);
            AudioSource source = sfxObj.AddComponent<AudioSource>();
            source.loop = false;
            source.playOnAwake = false;
            source.volume = 1f;
            _sfxPool.Add(source);
            return source;
        }

        /// <summary>
        /// Returns the first idle SFX source, or creates a new one if all sources
        /// are currently playing.
        /// </summary>
        private AudioSource GetAvailableSfxSource()
        {
            foreach (AudioSource source in _sfxPool)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }
            return CreateSfxSource();
        }

        // ── BGM ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Cross-fades from the current BGM to <paramref name="clip"/>.
        /// Stops any coroutine-based fade already in progress.
        /// </summary>
        /// <param name="clip">The new background music clip.</param>
        /// <param name="fadeDuration">Duration in seconds for the fade transition.</param>
        public void PlayBGM(AudioClip clip, float fadeDuration = 0.5f)
        {
            if (clip == null) return;

            if (_bgmFadeCoroutine != null)
            {
                StopCoroutine(_bgmFadeCoroutine);
            }
            _bgmFadeCoroutine = StartCoroutine(PlayBGMCoroutine(clip, fadeDuration));
        }

        private IEnumerator PlayBGMCoroutine(AudioClip clip, float fadeDuration)
        {
            // Fade out current.
            if (bgmSource.isPlaying)
            {
                yield return StartCoroutine(FadeVolume(bgmSource, 0f, fadeDuration));
                bgmSource.Stop();
            }

            // Switch clip and start.
            bgmSource.clip = clip;
            bgmSource.Play();

            // Fade in.
            yield return StartCoroutine(FadeVolume(bgmSource, bgmVolume, fadeDuration));
            _bgmFadeCoroutine = null;
        }

        /// <summary>
        /// Fades out the current BGM over <paramref name="fadeDuration"/> seconds,
        /// then stops playback.
        /// </summary>
        public void StopBGM(float fadeDuration = 0.5f)
        {
            if (_bgmFadeCoroutine != null)
            {
                StopCoroutine(_bgmFadeCoroutine);
            }
            _bgmFadeCoroutine = StartCoroutine(StopBGMCoroutine(fadeDuration));
        }

        private IEnumerator StopBGMCoroutine(float fadeDuration)
        {
            if (bgmSource.isPlaying)
            {
                yield return StartCoroutine(FadeVolume(bgmSource, 0f, fadeDuration));
                bgmSource.Stop();
            }
            _bgmFadeCoroutine = null;
        }

        // ── SFX ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Plays a one-shot sound effect from the SFX pool.
        /// </summary>
        /// <param name="clip">The audio clip to play.</param>
        /// <param name="volume">
        /// Relative volume scale (0-1) applied on top of the master SFX volume.
        /// Defaults to 1 (full master volume).
        /// </param>
        public void PlaySFX(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;

            AudioSource source = GetAvailableSfxSource();
            if (source != null)
            {
                source.PlayOneShot(clip, sfxVolume * Mathf.Clamp01(volume));
            }
        }

        // ── Volume Utilities ──────────────────────────────────────────────────

        /// <summary>
        /// Gets or sets the BGM volume (0-1). Changes are applied immediately
        /// and persisted to PlayerPrefs.
        /// </summary>
        public float BGMVolume
        {
            get => bgmVolume;
            set
            {
                bgmVolume = Mathf.Clamp01(value);
                if (bgmSource != null)
                {
                    bgmSource.volume = bgmVolume;
                }
                PlayerPrefs.SetFloat(BGM_VOL_KEY, bgmVolume);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// Gets or sets the SFX master volume (0-1). Changes are persisted
        /// to PlayerPrefs immediately.
        /// </summary>
        public float SFXVolume
        {
            get => sfxVolume;
            set
            {
                sfxVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(SFX_VOL_KEY, sfxVolume);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// Smoothly interpolates an AudioSource's volume from its current value
        /// to <paramref name="targetVolume"/> over <paramref name="duration"/> seconds.
        /// </summary>
        private IEnumerator FadeVolume(AudioSource source, float targetVolume, float duration)
        {
            float startVolume = source.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
                yield return null;
            }

            source.volume = targetVolume;
        }
    }
}
