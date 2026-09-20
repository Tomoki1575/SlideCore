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

    /// <summary> 曲の0秒にあたる dspTime の値(ノーツの位置・生成用タイマー) </summary>
    private double songStartDspTime;

    /// <summary> 曲の0秒にあたる realtimeSinceStartup の値(判定用タイマー) </summary>
    public double SongStartRealTime { get; private set; }

    public bool IsMusicPlaying { get; private set; } = false;

    public bool IsMusicStartReady { get; private set; } = false;

    private double pausedDspTime;

    private double pausedRealTime;

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
            CurrentSongTime = (float)(AudioSettings.dspTime - songStartDspTime);   //  正確な経過秒数 = (floatに変換)(現在の時刻 - 開始時の時刻)

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
        // 普通のタイマー（Time.time）が「1秒、2秒」と数えるのに対し、dspTime は裏側で「音の粒（サンプル）を何個処理したか」を数えている。例えば、
        // 音の粒 44100個分 ＝ 1秒
        // といったように、音のデータ量から時間を計算している。


        // AudioSettings.dspTime
        // メリット：音声システムを用いた時計を参照。そのため、音関係に関しては正確。
        // デメリット：入力時刻を持たないため自分で読むしかなく、その値もバッファ単位（20ms程度）でしか読み取らないため、実際に入力した入力時刻とずれる可能性が高い。
        // =>そのため、音をならす時計はこっちを採用。
        //
        // Time.realtimeSinceStartupAsDouble
        // メリット：OS単位で管理されているため、キー入力された時間を保持できる。つまりフレームの壁を越えて記録できる。
        // デメリット：長時間だと音（dspTime）に対して少しずつずれる。
        // =>そのため、入力判定を取る時計はこっちを採用

        double scheduleAhead = 0.2;                                  // 少し先に予約（読み込み・準備の余裕）

        songStartDspTime = AudioSettings.dspTime + scheduleAhead;               // 音が鳴る瞬間＝この時刻
        SongStartRealTime = Time.realtimeSinceStartupAsDouble + scheduleAhead;   // 入力用の時計も同じだけ未来へ

        audioSource.PlayScheduled(songStartDspTime);                     // その時刻ぴったりに鳴る
        IsMusicPlaying = true;
    }

    public void PauseSong()
    {
        if (!IsMusicPlaying) return;

        audioSource.Pause();

        // ポーズ画面に突入した際の時刻を記録
        pausedDspTime = AudioSettings.dspTime;
        pausedRealTime = Time.realtimeSinceStartupAsDouble;

        IsMusicPlaying = false;
    }

    public void ResumeSong()
    {
        // 曲の0秒にあたる時刻 += ゲームを起動してから今までの時間(常に動き続ける) - ポーズ画面に突入した際に記録した時刻
        // （メニューを見ていた時間もカウントダウンも、すべて「ゲームを起動してから今までの時間」に含まれる）
        //
        // 例えば、
        // AudioSettings.dspTimeやTime.realtimeSinceStartupAsDoubleがポーズが呼ばれた時点では100秒で
        // （これはポーズに突入した時点でpausedDspTimeやpausedRealTimeなどに記録されている）
        // ポーズ画面を20秒間開いたとすると、ポーズを閉じるときはまだ120秒。
        // ここからさらにカウントダウンの3秒が経過し、ResumeSong関数が呼ばれる頃には123秒になっている。
        //
        // これにより、
        // 曲の0秒にあたる時刻 += 現在の時刻 - ポーズ時に記録された時刻      即ち、
        // 曲の0秒にあたる時刻 += 123 - 100;
        // となって基準が23秒ぶん後ろへずれ、曲時間が23秒飛ぶのを防いでいる
        songStartDspTime += AudioSettings.dspTime - pausedDspTime;
        SongStartRealTime += Time.realtimeSinceStartupAsDouble - pausedRealTime;

        audioSource.UnPause();
        IsMusicPlaying = true;
    }
}
