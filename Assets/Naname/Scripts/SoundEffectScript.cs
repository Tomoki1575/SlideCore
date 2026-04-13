using UnityEngine;
using UnityEngine.Audio;

public class SoundEffectScript : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioClip tapSE;

    public static SoundEffectScript Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void TapNotesSound()
    {
        audioSource.PlayOneShot(tapSE);
    }
}