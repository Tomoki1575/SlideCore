using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    [SerializeField]
    private AudioMixer GameMixer;
    [SerializeField]
    private AudioSource MusicSource;
    [SerializeField]
    private AudioClip TapSound;
    [SerializeField]
    private AudioClip SlideSound;
    [SerializeField]
    private AudioClip NoiseSound;

    private const string MusicMixerName = "MusicVolume";
    private const string SEMixerName = "SEVolume";
    private const float SpecialPitch = 1.2f;

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

    public void SetSEVolume(float volume)
    {
        if (GameMixer) GameMixer.SetFloat(SEMixerName, ConvertDecibel(volume));
    }
    #endregion

    // Utility
    public void PauseAllSound()
    {

    }

    public void UnPauseAllSound()
    {

    }

    private float ConvertDecibel(float value)
    {
        value = Mathf.Clamp01(value);
        float decibel = 20f * Mathf.Log10(value);
        decibel = Mathf.Clamp(decibel, -80f, 0f);
        return decibel;
    }
}
