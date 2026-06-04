using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class InputManagerScript : MonoBehaviour
{
    [SerializeField] private InputActionAsset actions;

    private void OnEnable()
    {
        actions.Enable();

        actions.FindAction("Lane0").performed += ctx => OnTap(0, ctx);
        actions.FindAction("Lane1").performed += ctx => OnTap(1, ctx);
        actions.FindAction("Lane2").performed += ctx => OnTap(2, ctx);
        actions.FindAction("Lane3").performed += ctx => OnTap(3, ctx);
        actions.FindAction("Lane4").performed += ctx => OnTap(4, ctx);
        actions.FindAction("Lane5").performed += ctx => OnTap(5, ctx);

        actions.FindAction("SlideRight").performed += ctx => OnSlide(true, ctx);
        actions.FindAction("SlideLeft").performed += ctx => OnSlide(false,ctx);
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

            if (closestNoteMove.MyNotesType == NotesType.Slide)
            {
                if ((isRight && closestNoteMove.IsRight) || (!isRight && !closestNoteMove.IsRight))
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
