using TMPro;
using UnityEngine;

public class MusicManagerScript : MonoBehaviour
{
    public static MusicManagerScript Instance { get; private set; }

    [SerializeField] private AudioSource audioSource;

    [SerializeField] private MusicSelection musicSelection;

    [SerializeField] private TextMeshProUGUI songTimeText;

    public float CurrentSongTime { get; private set; }

    public float SongLength { get; private set; }

    public static float MsCurrentSongTime;

    private double dspStartTime;
    public bool IsMusicPlaying { get; private set; } = false;

    public static double SongStartRealTime;

    public bool IsMusicStartReady { get; private set; } = false;

    private void Awake()
    {
        CurrentSongTime = 0f;
        MsCurrentSongTime = 0f;

        Instance = this;

        // MusicSelection から音源を自動セット（譜面と音をズレさせないため）
        if (musicSelection != null && musicSelection.musicData != null && musicSelection.musicData.audioClip != null)
        {
            audioSource.clip = musicSelection.musicData.audioClip;
            audioSource.clip.LoadAudioData();

            // 曲の長さ
            SongLength = audioSource.clip.length;
        }
    }

    private void Update()
    {
        if (audioSource.clip == null)
            return;

        if (!IsMusicPlaying && audioSource.clip.loadState == AudioDataLoadState.Loaded)
        {
            IsMusicStartReady = true;
            return;
        }

        else if (IsMusicPlaying)
        {
            CurrentSongTime = (float)(AudioSettings.dspTime - dspStartTime);   //  正確な経過秒数 = (floatに変換)(現在の時刻 - 開始時の時刻)

            MsCurrentSongTime = Mathf.FloorToInt(CurrentSongTime * 1000);

            songTimeText.text = $"{MsCurrentSongTime}";
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
        IsMusicPlaying = true;
    }

    public void PauseSong()
    {
        audioSource.Pause();
        IsMusicPlaying = false;
    }
}
