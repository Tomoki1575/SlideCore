using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class InputManagerScript : MonoBehaviour
{
    [SerializeField] private InputActionAsset actions;

    private System.Action<CallbackContext> slideRightHandler, slideLeftHandler;
    private System.Action<CallbackContext> lane0Handler, lane1Handler,lane2Handler, lane3Handler, lane4Handler, lane5Handler;

    private void OnEnable()
    {
        actions.Enable();

        lane0Handler = ctx => OnTap(0, ctx);
        lane1Handler = ctx => OnTap(1, ctx);
        lane2Handler = ctx => OnTap(2, ctx);
        lane3Handler = ctx => OnTap(3, ctx);
        lane4Handler = ctx => OnTap(4, ctx);
        lane5Handler = ctx => OnTap(5, ctx);
        actions.FindAction("Lane0").performed += lane0Handler;
        actions.FindAction("Lane1").performed += lane1Handler;
        actions.FindAction("Lane2").performed += lane2Handler;
        actions.FindAction("Lane3").performed += lane3Handler;
        actions.FindAction("Lane4").performed += lane4Handler;
        actions.FindAction("Lane5").performed += lane5Handler;

        slideRightHandler = ctx => OnSlide(true, ctx);
        slideLeftHandler = ctx => OnSlide(false, ctx);
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

        actions.FindAction("SlideRight").performed -= slideRightHandler;
        actions.FindAction("SlideLeft").performed -= slideLeftHandler;

        actions.Disable();
    }

    /// <summary>
    /// レーン上の特定のボタンが押された時、呼ばれる関数。 {int 何番目のレーンか, InputAction.CallbackContext インプットシステムの専用変数}
    /// </summary>
    /// <param name="laneIndex">何番目のレーンか</param>
    /// <param name="context"></param>
    private void OnTap(int laneIndex, InputAction.CallbackContext context)
    {
        double exactTime = context.time;

        if (NoteGenerator.Instance == null || NoteGenerator.Instance.laneNotesLists == null) return;

        var targetLaneList = NoteGenerator.Instance.laneNotesLists[laneIndex];

        // そのレーンのリストにノーツが1つ以上存在する場合のみ処理する
        if (targetLaneList.Count > 0)
        {
            // リストの[0]番目は、そのレーンで「一番手前にいる」ノーツ
            MoveScript closestNoteMove = targetLaneList[0];

            if (closestNoteMove != null)
            {
                if (closestNoteMove.MyNotesType != NotesType.Tap) return;

                bool wasJudged = false;

                // ノーツにアタッチされているJudgeScriptを取得
                JudgeScript judgeScript = closestNoteMove.GetComponent<JudgeScript>();

                if (judgeScript != null)
                {
                    // OSが検知した正確な時間を渡して判定を実行する
                    wasJudged = judgeScript.ExecuteJudge(exactTime);

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
    /// スライドボタンが押された時、呼ばれる関数。 {bool 右スライドか？, InputAction.CallbackContext インプットシステムの専用変数}
    /// </summary>
    /// <param name="isRight">右スライドか？</param>
    /// <param name="context"></param>
    private void OnSlide(bool isRight, InputAction.CallbackContext context)
    {
        double exactTime = context.time;

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
            if (closestNoteMove.MyNotesType == NotesType.Slide)
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
