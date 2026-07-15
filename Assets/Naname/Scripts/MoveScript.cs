using UnityEngine;
using static GameDataManager;

public class MoveScript : MonoBehaviour
{
    public float HitTime;
    public int Lane;
    public NoteType MyNotesType;
    public bool IsRight;

    public static float ScrollSpeed = 7000f;
    private float judgmentLineY = -1900f;

    private RectTransform rectTransform;

    private JudgeScript judgeScript;
    private bool isMissTriggered = false; // 二重にMissが走らないためのガードフラグ

    // 理由：NotesGenerator が生成した直後にすぐ RectTransform を使えるようにするため
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Start()
    {
        judgeScript = GetComponent<JudgeScript>();
    }

    /// <summary>
    /// 現在ノーツがあるべき場所を計算し、その座標にノーツを置く関数。「ノーツを動かす関数」とも言える。
    /// </summary>
    public void RefreshPosition()
    {
        if (rectTransform == null) 
            return;

        // timeRemaining ←残り何秒で判定ラインに到達すべきかを記録する変数
        float timeRemaining = HitTime - MusicManagerScript.SongTime;

        // 「距離 ＝ 時間 × 速さ」 より、現在ノーツがあるべき座標を計算
        float noteYPos = judgmentLineY + (timeRemaining * ScrollSpeed);

        // 座標を更新
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, noteYPos);
    }

    void Update()
    {
        RefreshPosition();

        // timeRemaining ←残り何秒で判定ラインに到達すべきかを記録する変数
        float timeRemaining = HitTime - MusicManagerScript.SongTime;

        // check : オフセットなどを含む判定用の時間を計算
        float currentSongTime = (float)(Time.realtimeSinceStartupAsDouble - MusicManagerScript.SongStartRealTime) - JudgeScript.InputOffset;

        if(!isMissTriggered && timeRemaining <= 0 && MyNotesType == NoteType.Noise)
        {
            isMissTriggered = true;

            judgeScript.ExecuteNoiseJudge();

            return;
        }

        if (!isMissTriggered && currentSongTime > HitTime + 0.4f)
        {
            isMissTriggered = true;

            if (judgeScript != null)
            {
                judgeScript.TriggerMissByThrough();
            }
        }
    }
}