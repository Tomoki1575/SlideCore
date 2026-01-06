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
    private int InitialSize = 16;
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
        RequireCheck.ThrowIfAnyNull(this,
            (GameMixer, nameof(GameMixer)),
            (MusicSource, nameof(MusicSource)),
            (SEMixerGroup, nameof(SEMixerGroup)),
            (OwnerGameObject, nameof(OwnerGameObject)),
            (TapSound, nameof(TapSound)),
            (HoldSound, nameof(HoldSound)),
            (SlideSound, nameof(SlideSound)),
            (NoiseSound, nameof(NoiseSound))
            );

        SEPool = new AudioSourcePool(OwnerGameObject, SEMixerGroup, InitialSize);
    }

    #region Music
    public void PlayBGM() => MusicSource.Play();

    public void PauseBGM() => MusicSource.Pause();

    public void UnPauseBGM() => MusicSource.UnPause();

    public void SetBGMClip(AudioClip clip)
    {
        clip.LoadAudioData();
        MusicSource.clip = clip;
    }

    public void SetBGMTime(double time)
    {
        MusicSource.time = (float)time;
    }

    public double GetBGMTime()
    {
        return (double)MusicSource.time;
    }

    public bool IsBGMPlaying() => MusicSource.isPlaying;

    public void SetBGMVolume(float volume) => GameMixer.SetFloat(MusicMixerName, ConvertDecibel(volume));
    #endregion

    #region SE
    public void PlayTapSE(bool bSpecial = false)
    {
        float pitch = bSpecial ? SpecialPitch : 1f;
        PlayOneShotSE(TapSound, pitch);
    }

    public LoopHandle PlayHoldSE(bool bSpecial = false)
    {
        float pitch = bSpecial ? SpecialPitch : 1f;
        return PlayLoopSE(HoldSound, pitch);
    }

    public void StopHoldSE(LoopHandle loopHandle)
    {
        StopLoopSE(loopHandle);
    }

    public void PlaySlideSE(bool bSpecial = false)
    {
        float pitch = bSpecial ? SpecialPitch : 1f;
        PlayOneShotSE(SlideSound, pitch);
    }

    public void PlayNoiseSE(bool bSpecial = false)
    {
        float pitch = bSpecial ? SpecialPitch : 1f;
        PlayOneShotSE(NoiseSound, pitch);
    }

    public void SetSEVolume(float volume) => GameMixer.SetFloat(SEMixerName, ConvertDecibel(volume));
    #endregion

    #region Utility
    public void PauseAllSound()
    {
        PauseBGM();
        foreach (AudioSource src in SEPool.ActiveSources) src.Pause();
    }

    public void UnPauseAllSound()
    {
        UnPauseBGM();
        foreach (AudioSource src in SEPool.ActiveSources) src.UnPause();
    }

    private void PlayOneShotSE(AudioClip clip, float pitch = 1.0f, float volume = 1.0f)
    {
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
        SEPool.Release(src);
    }

    private LoopHandle PlayLoopSE(AudioClip clip, float pitch = 1.0f, float volume = 1.0f)
    {
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
