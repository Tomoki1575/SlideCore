using UnityEngine;

public class PreviewSongScript : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    private AudioClip currentClip;

    public static PreviewSongScript Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 曲の選択が変わった時に呼ばれる
    /// </summary>
    public void PlayPreviewSong(MusicData music)
    {
        if (music == null)
            return;

        AudioClip clip = music.previewClip;

        // preview用の曲が入っていない場合、曲を止める
        if (clip == null)
        {
            audioSource.Stop();
            currentClip = null;
            return;
        }

        // 同じ曲を選び直した時に、鳴らし直さない
        if (currentClip == clip)
            return;

        currentClip = clip;

        audioSource.clip = clip;
        audioSource.Play();
    }
}
