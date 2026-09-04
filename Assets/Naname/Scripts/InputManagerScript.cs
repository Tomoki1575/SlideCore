using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
using static GameDataManager;
using UnityEngine.SceneManagement;

public class InputManagerScript : MonoBehaviour
{
    [SerializeField] private InputActionAsset actions;

    public static bool[] isLanePressed = new bool[6];

    [SerializeField] private float slideCooldown = 0.05f;   // スライドの最小間隔（秒・調整可）
    private double lastRightSlideTime = -999;
    private double lastLeftSlideTime = -999;

    private System.Action<CallbackContext> lane0Handler, lane1Handler, lane2Handler, lane3Handler, lane4Handler, lane5Handler;
    private System.Action<CallbackContext> lane0ReleaseHandler, lane1ReleaseHandler, lane2ReleaseHandler, lane3ReleaseHandler, lane4ReleaseHandler, lane5ReleaseHandler;
    private System.Action<CallbackContext> slideRightHandler, slideLeftHandler;

    private void OnEnable()
    {
        actions.Enable();

        lane0Handler = ctx => OnLaneTap(0, ctx);
        lane1Handler = ctx => OnLaneTap(1, ctx);
        lane2Handler = ctx => OnLaneTap(2, ctx);
        lane3Handler = ctx => OnLaneTap(3, ctx);
        lane4Handler = ctx => OnLaneTap(4, ctx);
        lane5Handler = ctx => OnLaneTap(5, ctx);
        actions.FindAction("Lane0").performed += lane0Handler;
        actions.FindAction("Lane1").performed += lane1Handler;
        actions.FindAction("Lane2").performed += lane2Handler;
        actions.FindAction("Lane3").performed += lane3Handler;
        actions.FindAction("Lane4").performed += lane4Handler;
        actions.FindAction("Lane5").performed += lane5Handler;

        lane0ReleaseHandler = ctx => OnLaneRelease(0);
        lane1ReleaseHandler = ctx => OnLaneRelease(1);
        lane2ReleaseHandler = ctx => OnLaneRelease(2);
        lane3ReleaseHandler = ctx => OnLaneRelease(3);
        lane4ReleaseHandler = ctx => OnLaneRelease(4);
        lane5ReleaseHandler = ctx => OnLaneRelease(5);
        actions.FindAction("Lane0").canceled += lane0ReleaseHandler;
        actions.FindAction("Lane1").canceled += lane1ReleaseHandler;
        actions.FindAction("Lane2").canceled += lane2ReleaseHandler;
        actions.FindAction("Lane3").canceled += lane3ReleaseHandler;
        actions.FindAction("Lane4").canceled += lane4ReleaseHandler;
        actions.FindAction("Lane5").canceled += lane5ReleaseHandler;

        slideRightHandler = ctx => OnLaneSlide(true, ctx);
        slideLeftHandler = ctx => OnLaneSlide(false, ctx);
        actions.FindAction("SlideRight").performed += slideRightHandler;
        actions.FindAction("SlideLeft").performed += slideLeftHandler;
    }

    private void OnDisable()
    {
        actions.FindAction("Lane0").performed -= lane0Handler;
        actions.FindAction("Lane1").performed -= lane1Handler;
        actions.FindAction("Lane2").performed -= lane2Handler;
        actions.FindAction("Lane3").performed -= lane3Handler;
        actions.FindAction("Lane4").performed -= lane4Handler;
        actions.FindAction("Lane5").performed -= lane5Handler;

        actions.FindAction("Lane0").canceled -= lane0ReleaseHandler;
        actions.FindAction("Lane1").canceled -= lane1ReleaseHandler;
        actions.FindAction("Lane2").canceled -= lane2ReleaseHandler;
        actions.FindAction("Lane3").canceled -= lane3ReleaseHandler;
        actions.FindAction("Lane4").canceled -= lane4ReleaseHandler;
        actions.FindAction("Lane5").canceled -= lane5ReleaseHandler;

        actions.FindAction("SlideRight").performed -= slideRightHandler;
        actions.FindAction("SlideLeft").performed -= slideLeftHandler;

        actions.Disable();
    }

    /// <summary>
    /// レーン上の特定のボタンが押された時、呼ばれる関数 {int 何番目のレーンか, InputAction.CallbackContext インプットシステムの専用変数}
    /// </summary>
    /// <param name="laneIndex">何番目のレーンか</param>
    /// <param name="context"></param>
    private void OnLaneTap(int laneIndex, InputAction.CallbackContext context)
    {
        double exactTime = context.time;

        isLanePressed[laneIndex] = true;

        if (NoteGenerator.Instance == null || NoteGenerator.Instance.laneNotesLists == null) return;

        var targetLaneList = NoteGenerator.Instance.laneNotesLists[laneIndex];

        // そのレーンのリストにノーツが1つ以上存在する場合のみ処理する
        if (targetLaneList.Count > 0)
        {
            // リストの[0]番目は、そのレーンで「一番手前にいる」ノーツ
            MoveScript closestNoteMove = targetLaneList[0];

            if (closestNoteMove != null)
            {
                if (closestNoteMove.MyNotesType != NoteType.Tap && closestNoteMove.MyNotesType != NoteType.Hold)
                    return;

                bool wasJudged = false;

                // ノーツにアタッチされているJudgeScriptを取得
                JudgeScript judgeScript = closestNoteMove.GetComponent<JudgeScript>();

                if (judgeScript != null)
                {
                    if (closestNoteMove.MyNotesType == NoteType.Hold)
                    {
                        wasJudged = judgeScript.ExecuteHoldStartJudge(exactTime);
                    }

                    else
                    {
                        // OSが検知した正確な時間を渡して判定を実行する
                        wasJudged = judgeScript.ExecuteJudge(exactTime);
                    }

                    if (wasJudged)
                    {
                        // これをしないと、次のノーツを叩きに行ったときに古いノーツが邪魔をする
                        targetLaneList.RemoveAt(0);
                    }
                }
            }
        }
    }

    /// <summary>
    /// レーンのボタンが離された瞬間に呼ばれる。押下フラグを下ろすだけ。
    /// </summary>
    private void OnLaneRelease(int laneIndex)
    {
        isLanePressed[laneIndex] = false;
    }

    /// <summary>
    /// スライドボタンが押された時、呼ばれる関数 {bool 右スライドか？, InputAction.CallbackContext インプットシステムの専用変数}
    /// </summary>
    /// <param name="isRight">右スライドか？</param>
    /// <param name="context"></param>
    private void OnLaneSlide(bool isRight, InputAction.CallbackContext context)
    {
        double exactTime = context.time;

        // 同じ方向のスライドが短時間に連続したら無視（多重判定防止）
        double lastTime = isRight ? lastRightSlideTime : lastLeftSlideTime;

        if (exactTime - lastTime < slideCooldown)
            return;

        if (isRight) 
            lastRightSlideTime = exactTime;

        else
            lastLeftSlideTime = exactTime;

        // レーンを動かす処理
        if (LaneScript.Instance != null)
        {
            LaneScript.Instance.SlideLane(isRight);
        }

        // レーン移動に合わせてスライドノーツの判定を行う処理
        if (NoteGenerator.Instance == null || NoteGenerator.Instance.laneNotesLists == null) return;

        for (int i = 0; i < 6; i++)
        {
            // 6レーン全てのリスト内に叩けるノーツがあるかチェック（スライドノーツの判定は全てのレーンを共有するため）
            var targetLaneList = NoteGenerator.Instance.laneNotesLists[i];
            if (targetLaneList.Count == 0) continue;

            // リストの[0]番目は、そのレーンで「一番手前にいる」ノーツ
            MoveScript closestNoteMove = targetLaneList[0];
            if (closestNoteMove == null) continue;

            // スライド
            if (closestNoteMove.MyNotesType == NoteType.Slide)
            {
                if (isRight == closestNoteMove.IsRight)
                {
                    JudgeScript judgeScript = closestNoteMove.GetComponent<JudgeScript>();
                    if (judgeScript != null)
                    {
                        bool wasJudged = judgeScript.ExecuteJudge(exactTime);

                        if (wasJudged)
                        {
                            targetLaneList.RemoveAt(0);
                        }
                    }
                }
            }
        }
    }
}
