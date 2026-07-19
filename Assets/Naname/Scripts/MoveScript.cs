using UnityEngine;
using static GameDataManager;

public class MoveScript : MonoBehaviour
{
    public float HitTime;
    public int Lane;
    public NoteType MyNotesType;
    public bool IsRight;

    public float EndHitTime;

    public static float ScrollSpeed = 7000f;
    private float judgmentLineY = -1900f;

    private RectTransform rectTransform;

    private JudgeScript judgeScript;
    private bool isMissTriggered = false; // 二重にMissが走らないためのガードフラグ

    public RectTransform HoldBand;              // 帯（始点ノーツだけが持つ）
    private const float MaxBandTopY = 2600f;    // 帯の上端の限界
    public float HoldBaseOffsetY;               // 帯の下端

    // 理由：NotesGenerator が生成した直後にすぐ RectTransform を使えるようにするため
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Start()
    {
        judgeScript = GetComponent<JudgeScript>();
    }

    void Update()
    {
        RefreshPosition();

        if (MyNotesType == NoteType.Hold)
            RefreshHoldBand();

        // timeRemaining ←残り何秒で判定ラインに到達すべきかを記録する変数
        float timeRemaining = HitTime - MusicManagerScript.SongTime;

        // check : オフセットなどを含む判定用の時間を計算
        float currentSongTime = (float)(Time.realtimeSinceStartupAsDouble - MusicManagerScript.SongStartRealTime) - JudgeScript.InputOffset;

        if (!isMissTriggered && timeRemaining <= 0 && MyNotesType == NoteType.Noise)
        {
            isMissTriggered = true;

            judgeScript.ExecuteNoiseJudge();

            return;
        }

        float missDeadline = (MyNotesType == NoteType.Hold) ? EndHitTime : HitTime + 0.4f;

        if (!isMissTriggered && currentSongTime > missDeadline)
        {
            isMissTriggered = true;

            if (judgeScript != null)
            {
                judgeScript.TriggerMissByThrough();
            }
        }
    }

    /// <summary>
    /// 現在ノーツがあるべき場所を計算し、その座標にノーツを配置する関数。
    /// 「ノーツを動かす関数」とも言える。
    /// </summary>
    public void RefreshPosition()
    {
        // 「距離 ＝ 時間 × 速さ」 より、現在ノーツがあるべき座標を計算
        float noteYPos = judgmentLineY + ((HitTime - MusicManagerScript.SongTime) * ScrollSpeed);

        // ホールドの始点は判定ラインより下へは行かず貼り付く（見た目だけ）
        if (MyNotesType == NoteType.Hold && noteYPos < judgmentLineY)
        {
            noteYPos = judgmentLineY;
        }

        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, noteYPos);        // 座標を更新
    }

    /// <summary>
    /// ホールドノーツの始点および帯部分の配置を決定する関数
    /// </summary>
    public void RefreshHoldBand()
    {
        // ホールド終点の現在位置を時間から直接計算（距離 ＝ 時間 × 速さ）
        float endY = judgmentLineY + (EndHitTime - MusicManagerScript.SongTime) * ScrollSpeed;

        float bandBottom = rectTransform.anchoredPosition.y + HoldBaseOffsetY;      // 帯の下端（始点に張り付く）
        float visibleTop = Mathf.Min(endY, MaxBandTopY);     // 上端を画面外に出さないようにする
        float visibleHeight = Mathf.Max(0f, visibleTop - bandBottom);   // 今表示する帯の高さ（消費で縮む）

        HoldBand.sizeDelta = new Vector2(HoldBand.sizeDelta.x, visibleHeight);  //帯の長さを決定
    }
}