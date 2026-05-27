using UnityEngine;
using UnityEngine.InputSystem;

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
    }

    /// <summary>
    /// 特定のボタンが押された時、呼ばれる変数。 {int 何番目のレーンか, InputAction.CallbackContext インプットシステムの専用変数}
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
            // リストの[0]番目は、そのレーンで「一番最初に生まれた（＝一番手前にいる）」ノーツ
            MoveScript closestNoteMove = targetLaneList[0];

            if (closestNoteMove != null)
            {
                // ノーツにアタッチされている JudgeScript を取得
                JudgeScript judgeScript = closestNoteMove.GetComponent<JudgeScript>();

                if (judgeScript != null)
                {
                    // OSが検知した正確な時間を渡して判定を実行する
                    judgeScript.ExecuteJudge(exactTime);

                    // これをしないと、次のノーツを叩きに行ったときに古いノーツが邪魔をする
                    targetLaneList.RemoveAt(0);
                }
            }
        }
    }
}
