using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioSourcePool : MonoBehaviour
{
    [SerializeField]
    private int InitialSize = 16;
    [SerializeField]
    private AudioMixerGroup AudioMixer;

    private Queue<AudioSource> IdleAudioSource = new();
    private HashSet<AudioSource> ActiveAudioSource = new();

    public IEnumerable<AudioSource> ActiveSources => ActiveAudioSource;

    private void Awake()
    {
        for (int i = 0; i < InitialSize; i++)
            IdleAudioSource.Enqueue(Create());
    }

    private AudioSource Create()
    {
        GameObject go = new GameObject("PooledAudioSource");
        go.transform.parent = transform;

        AudioSource src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.outputAudioMixerGroup = AudioMixer;
        return src;
    }

    public AudioSource Get()
    {
        // ‹ó‚«‚ª‚ ‚é‚È‚çŽæ‚èo‚·A‹ó‚«‚ª‚È‚¢‚È‚çV‹Kì¬
        AudioSource src = IdleAudioSource.Count > 0 ? IdleAudioSource.Dequeue() : Create();
        ActiveAudioSource.Add(src);
        return src;
    }

    public void Release(AudioSource src)
    {
        if (!ActiveAudioSource.Remove(src)) return;

        src.Stop();
        src.clip = null;
        src.loop = false;
        src.pitch = 1.0f;

        IdleAudioSource.Enqueue(src);
    }
}
