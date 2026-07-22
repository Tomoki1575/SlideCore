using UnityEngine;
using static GameDataManager;

public class MoveScript : MonoBehaviour
{
    public float HitTime;
    public int Lane;
    public NoteType MyNotesType;
    public bool IsRight;

    public float EndHitTime;

    public static float ScrollSpeed = 2500f;
    private float judgmentLineY = -1900f;

    private RectTransform rectTransform;

    private JudgeScript judgeScript;
    private bool isMissTriggered = false; // 二重にMissが走らないためのガードフラグ

    public RectTransform HoldBand;              // 帯（始点ノーツだけが持つ）
    private const float MaxBandTopY = 2600f;    // 帯の上端の限界
    public float HoldBaseOffsetY;               // 帯の下端

    private float heldTime = 0f;   // 帯の区間中、レーンが押されていた累積時間（帯部分のコンボの分子）

    private bool holdStartJudged = false;

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


        // ノーツを消す処理
        if (!isMissTriggered)
        {
            switch (MyNotesType)
            {
                case NoteType.Noise:
                    // ノイズの場合、叩かれるべき時間が過ぎた瞬間判定されるため、その瞬間消す
                    if (timeRemaining <= 0)
                    {
                        isMissTriggered = true;

                        judgeScript.ExecuteNoiseJudge();
                    }
                    break;

                case NoteType.Hold:
                    float songTime = MusicManagerScript.SongTime;

                    // 始点をスルーした場合、始点の判定窓を過ぎたらmiss
                    if(!holdStartJudged && currentSongTime > HitTime + JudgeScript.MissWindow)
                    {
                        holdStartJudged = true;
                        judgeScript.TriggerHoldStartMiss();
                    }

                    // 帯の区間中（始点〜終点）で、レーンが押されていれば押し時間を貯める
                    if (songTime >= HitTime && songTime < EndHitTime && InputManagerScript.isLanePressed[Lane])
                    {
                        heldTime += Time.deltaTime;
                    }


                    // 帯の部分が流れ終わってから、貼り付けておいた始点を消す
                    if (MusicManagerScript.SongTime >= EndHitTime)
                    {
                        isMissTriggered = true;

                        float bandDuration = EndHitTime - HitTime;
                        float heldRatio = bandDuration > 0f ? heldTime / bandDuration : 0f;

                        judgeScript.ExecuteHoldEndJudge(heldRatio);
                    }
                    break;

                default:
                    // ホールド以外のノーツにおいて、叩かれずにそのまま流れたら（遅missの判定になる時間まで叩かれなかったら）ノーツを消す
                    if (currentSongTime > HitTime + JudgeScript.MissWindow)
                    {
                        isMissTriggered = true;
                        judgeScript.TriggerMissByThrough();
                    }
                    break;
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

    /// <summary>
    /// 始点を押して判定できたとき、JudgeScriptから呼ばれる関数
    /// </summary>
    public void MarkHoldStarted()
    {
        holdStartJudged = true;
    }
}