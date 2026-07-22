using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MusicManagerScript : MonoBehaviour
{
    public static MusicManagerScript Instance;

    [SerializeField] private AudioSource audioSource;

    [SerializeField] private MusicSelection musicSelection;

    [SerializeField] private TextMeshProUGUI songTimeText;

    public static float SongTime;
    public static float MsSongTime;

    private double dspStartTime;
    private bool isPlaying = false;

    public static double SongStartRealTime;

    private float resultDelay = 1.5f;

    private void Awake()
    {
        SongTime = 0f;
        MsSongTime = 0f;

        Instance = this;
    }

    private void Start()
    {
        // MusicSelection から音源を自動セット（譜面と音をズレさせないため）
        if (musicSelection != null && musicSelection.musicData != null)
        {
            audioSource.clip = musicSelection.musicData.audioClip;
            audioSource.clip.LoadAudioData();
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

        double scheduleAhead = 0.2;                                  // 少し先に予約（読み込み・準備の余裕）

        dspStartTime = AudioSettings.dspTime + scheduleAhead;               // 音が鳴る瞬間＝この時刻
        SongStartRealTime = Time.realtimeSinceStartupAsDouble + scheduleAhead;   // 入力用の時計も同じだけ未来へ

        audioSource.PlayScheduled(dspStartTime);                     // その時刻ぴったりに鳴る
        isPlaying = true;
    }

    private void Update()
    {
        if (!isPlaying && audioSource.clip.loadState == AudioDataLoadState.Loaded)
        {
            StartSong();

            return;
        }

        else if (isPlaying)
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