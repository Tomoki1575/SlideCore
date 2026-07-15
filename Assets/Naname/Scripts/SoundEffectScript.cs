using UnityEngine;
using static GameDataManager;
public class SoundEffectScript : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private AudioClip tapSE;
    [SerializeField] private AudioClip slideSE;

    public static SoundEffectScript Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void TapNotesSound(NoteType notesType)
    {
        switch (notesType)
        {
            case NoteType.Tap:
                audioSource.PlayOneShot(tapSE);
                break;
            case NoteType.Slide:
                audioSource.PlayOneShot(slideSE);
                break;
        }

    }
}