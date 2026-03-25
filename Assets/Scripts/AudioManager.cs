using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip backgroundMusic;
    public AudioClip jump;
    public AudioClip walk;
    public AudioClip portal;
    public AudioClip death;
    public AudioClip drop;
    public AudioClip saw;
    public AudioClip press;




    [Header("Death Clips Folder (Resources)")]
    public string deathClipsFolder = "Special Sounds";

    [Header("Death Sound Settings")]
    [Range(0f, 2f)]
    public float deathVolumeMultiplier = 1.5f;
    [Range(0f, 1f)]
    public float musicDuckVolume = 0.05f;
    public float musicFadeBackDuration = 0.3f;

    private AudioClip[] deathClips;
    private List<int> deathClipBag = new List<int>();

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

        // FIX: Ensure sfxSource is at full volume
        if (sfxSource != null)
            sfxSource.volume = 1f;

        LoadDeathClips();
    }

    void Start()
    {
        PlayBackgroundMusic();
    }

    private void LoadDeathClips()
    {
        deathClips = Resources.LoadAll<AudioClip>(deathClipsFolder);
        Debug.Log("[AudioManager] Death clips found: " + deathClips.Length);
        RefillBag();
    }

    private void RefillBag()
    {
        deathClipBag.Clear();
        if (deathClips == null || deathClips.Length == 0) return;

        List<int> indices = new List<int>();
        for (int i = 0; i < deathClips.Length; i++)
            indices.Add(i);

        for (int i = indices.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int tmp = indices[i];
            indices[i] = indices[j];
            indices[j] = tmp;
        }

        deathClipBag = indices;
    }

    private AudioClip GetNextDeathClip()
    {
        if (deathClipBag.Count == 0)
            RefillBag();

        if (deathClipBag.Count == 0) return null;

        int index = deathClipBag[deathClipBag.Count - 1];
        deathClipBag.RemoveAt(deathClipBag.Count - 1);
        return deathClips[index];
    }

    public void PlayBackgroundMusic()
    {
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip, volume);
    }

    public void PlayRandomDeathSound()
    {
        AudioClip folderClip = GetNextDeathClip();
        StartCoroutine(PlayDeathWithDucking(death, folderClip));
    }

    private IEnumerator PlayDeathWithDucking(AudioClip firstClip, AudioClip secondClip)
    {
        float originalVolume = musicSource != null ? musicSource.volume : 1f;

        if (musicSource != null)
            musicSource.volume = musicDuckVolume;

        // FIX: Removed Clamp01 so deathVolumeMultiplier can exceed 1.0
        if (firstClip != null)
        {
            PlaySFX(firstClip, deathVolumeMultiplier);
            yield return new WaitForSeconds(firstClip.length);
        }

        // FIX: Removed Clamp01 so deathVolumeMultiplier can exceed 1.0
        if (secondClip != null)
        {
            PlaySFX(secondClip, deathVolumeMultiplier);
            yield return new WaitForSeconds(secondClip.length);
        }

        if (musicSource != null)
        {
            float elapsed = 0f;
            while (elapsed < musicFadeBackDuration)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(musicDuckVolume, originalVolume, elapsed / musicFadeBackDuration);
                yield return null;
            }
            musicSource.volume = originalVolume;
        }
    }
}