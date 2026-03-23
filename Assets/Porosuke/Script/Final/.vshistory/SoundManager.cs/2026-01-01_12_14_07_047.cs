using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    [SerializeField]
    private AudioMixer GameMixer;
    [SerializeField]
    private AudioSource MusicSource;
    [SerializeField]
    private AudioMixerGroup SEMixerGroup;
    [SerializeField]
    private int InitialSize;
    [SerializeField]
    private GameObject OwnerGameObject;
    [SerializeField]
    private AudioClip TapSound;
    [SerializeField]
    private AudioClip HoldSound;
    [SerializeField]
    private AudioClip SlideSound;
    [SerializeField]
    private AudioClip NoiseSound;

    private AudioSourcePool SEPool;

    private const string MusicMixerName = "MusicVolume";
    private const string SEMixerName = "SEVolume";
    private const float SpecialPitch = 1.2f;

    public sealed class LoopHandle
    {
        internal AudioSource Source;
    }

    private void Awake()
    {
        if(OwnerGameObject != null && SEMixerGroup != null)
            SEPool = new AudioSourcePool(OwnerGameObject, SEMixerGroup, InitialSize);
    }

    #region Music
    public void PlayBGM()
    {
        if (MusicSource) MusicSource.Play();
    }

    public void PauseBGM()
    {
        if (MusicSource) MusicSource.Pause();
    }

    public void UnPauseBGM()
    {
        if (MusicSource) MusicSource.UnPause();
    }

    public void SetBGMClip(AudioClip clip)
    {
        clip.LoadAudioData();
        if (MusicSource) MusicSource.clip = clip;
    }

    public bool IsBGMPlaying()
    {
        if (MusicSource) return MusicSource.isPlaying;
        else return false;
    }

    public void SetBGMVolume(float volume)
    {
        if (GameMixer) GameMixer.SetFloat(MusicMixerName, ConvertDecibel(volume));
    }
    #endregion

    #region SE
    public void PlayTapSE(bool bSpecial = false)
    {
        float pitch = bSpecial ? SpecialPitch : 1f;
        if (TapSound) PlayOneShotSE(TapSound, pitch);
    }

    public LoopHandle PlayHoldSE(bool bSpecial = false)
    {
        float pitch = bSpecial ? SpecialPitch : 1f;
        if (HoldSound) return PlayLoopSE(HoldSound, pitch);
        else return null;
    }

    public void StopHoldSE(LoopHandle loopHandle)
    {
        StopLoopSE(loopHandle);
    }

    public void PlaySlideSE(bool bSpecial = false)
    {
        float pitch = bSpecial ? SpecialPitch : 1f;
        if (SlideSound) PlayOneShotSE(SlideSound, pitch);
    }

    public void PlayNoiseSE(bool bSpecial = false)
    {
        float pitch = bSpecial ? SpecialPitch : 1f;
        if (NoiseSound) PlayOneShotSE(NoiseSound, pitch);
    }

    public void SetSEVolume(float volume)
    {
        if (GameMixer) GameMixer.SetFloat(SEMixerName, ConvertDecibel(volume));
    }
    #endregion

    #region Utility
    public void PauseAllSound()
    {
        PauseBGM();
        if (SEPool == null) return;
        foreach (AudioSource src in SEPool.ActiveSources) src.Pause();
    }

    public void UnPauseAllSound()
    {
        UnPauseBGM();
        if (SEPool == null) return;
        foreach (AudioSource src in SEPool.ActiveSources) src.UnPause();
    }

    private void PlayOneShotSE(AudioClip clip, float pitch = 1.0f, float volume = 1.0f)
    {
        if (SEPool == null) return;
        AudioSource src = SEPool.Get();

        src.clip = clip;
        src.loop = false;
        src.pitch = pitch;
        src.volume = volume;

        src.Play();
        // Ä¶I—¹Œã‚ÉPool‚ÉŽ©“®•Ô‹p‚·‚é
        StartCoroutine(ReturnWhenFinished(src));
    }

    private IEnumerator ReturnWhenFinished(AudioSource src)
    {
        yield return new WaitWhile(() => src.isPlaying);
        if (SEPool != null) SEPool.Release(src);
    }

    private LoopHandle PlayLoopSE(AudioClip clip, float pitch = 1.0f, float volume = 1.0f)
    {
        if (SEPool == null) return null;
        AudioSource src = SEPool.Get();

        src.clip = clip;
        src.loop = true;
        src.pitch = pitch;
        src.volume = volume;

        src.Play();
        return new LoopHandle { Source = src };
    }

    private void StopLoopSE(LoopHandle handle)
    {
        if (handle == null || handle.Source == null) return;
        if (SEPool == null) return;

        SEPool.Release(handle.Source);
        handle.Source = null;
    }

    private float ConvertDecibel(float value)
    {
        value = Mathf.Clamp01(value);
        float decibel = 20f * Mathf.Log10(value);
        decibel = Mathf.Clamp(decibel, -80f, 0f);
        return decibel;
    }
    #endregion
}
