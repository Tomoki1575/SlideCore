using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MusicManagerScript : MonoBehaviour
{
    public static MusicManagerScript Instance;

    [SerializeField] private AudioSource audioSource;

    [SerializeField] private TextMeshProUGUI songTimeText;

    [SerializeField] private MusicSelection musicSelection;

    public static float SongTime;
    public static float MsSongTime;

    private double dspStartTime;
    private bool isPlaying = false;

    public static double SongStartRealTime;

    private float resultDelay = 1.5f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // MusicSelection から音源を自動セット（譜面と音をズレさせないため）
        if (musicSelection != null && musicSelection.musicData != null)
        {
            audioSource.clip = musicSelection.musicData.audioClip;
        }
    }

    /// <summary>
    /// 曲開始時の時間を保持し、曲を再生する関数。
    /// </summary>
    public void StartSong()
    {
        // [ AudioSettings.dspTime; について ]
        // OSそのものの音をベースに作られたタイマー
        //
        // 普通のタイマー（Time.time）が「1秒、2秒」と数えるのに対し、dspTime は裏側で「音の粒（サンプル）を何個処理したか」を数えている
        // 音の粒 44100個分 ＝ 1秒
        // といったように、音のデータ量から時間を計算している

        dspStartTime = AudioSettings.dspTime;

        SongStartRealTime = Time.realtimeSinceStartupAsDouble;

        audioSource.Play();
        isPlaying = true;
    }

    private void Update()
    {
        if (Keyboard.current.pKey.wasPressedThisFrame && !isPlaying)
        {
            StartSong();
        }

        if (isPlaying)
        {
            SongTime = (float)(AudioSettings.dspTime - dspStartTime);   //  正確な経過秒数 = (floatに変換)(現在の時刻 - 開始時の時刻)

            MsSongTime = Mathf.FloorToInt(SongTime * 1000);

            songTimeText.text = $"{MsSongTime}";

            // 曲の長さ＋余韻を過ぎたらリザルトへ
            if (SongTime >= audioSource.clip.length + resultDelay)
            {
                isPlaying = false;
                SceneManager.LoadScene("ResultScene");
            }
        }
    }
}