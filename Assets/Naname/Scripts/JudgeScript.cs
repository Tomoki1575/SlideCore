using UnityEngine;
using static GameDataManager;

public enum JudgeResult
{
    Perfect,
    Great,
    Good,
    Miss
}

public class JudgeScript : MonoBehaviour
{
    private MoveScript moveScript;

    public const float PerfectWindow = 0.07f;
    public const float GreatWindow = 0.14f;
    public const float GoodWindow = 0.21f;
    public const float MissWindow = 0.4f;

    public static float InputOffset { get; private set; } = 0.1f;

    void Start()
    {
        moveScript = GetComponent<MoveScript>();
    }

    /// <summary>
    /// 押された時間を受け取り、「UI表示+SE再生+ノーツ破棄」を行う。
    /// その後、それが空振りだったか空振りじゃなかったかを返す関数。
    /// （ホールドノーツ以外）
    /// </summary>
    /// <param name="pressedTime">新Input Systemが記録した、物理的にキーが押された正確な時間(context.time)</param>
    public bool ExecuteJudge(double pressedTime)
    {
        JudgeResult? result = JudgeByTime(pressedTime, out bool isLate, out float timeUntilHit);

        if (result == null) // 空振り
            return false;

        OnNotesJudged(result.Value, isLate, timeUntilHit, moveScript.MyNotesType, true, true);  // UI/SE + 破棄
        return true;
    }

    /// <summary>
    /// （ホールドノーツ専用）
    /// 押された時間を受け取り、「UI表示+SE再生」を行う。（ホールドノーツは始点を即座に破棄はしない。）
    /// その後、それが空振りだったか空振りじゃなかったかを返す関数。
    /// </summary>
    /// <param name="pressedTime">新Input Systemが記録した、物理的にキーが押された正確な時間(context.time)</param>

    public bool ExecuteHoldStartJudge(double pressedTime)
    {
        JudgeResult? result = JudgeByTime(pressedTime, out bool isLate, out float timeUntilHit);

        if (result == null)     // 空振り
            return false;

        OnNotesJudged(result.Value, isLate, timeUntilHit, moveScript.MyNotesType, destroy: false,true);  // UI/SEだけ（破棄はしない）。

        moveScript.MarkHoldStarted();   // 始点が押せたことを記録

        return true;
    }

    public void ExecuteHoldEndJudge(float heldRatio)
    {
        // まだリストに残っていれば外す（始点を一度も押さなかったHold対策）
        if (NoteGenerator.Instance != null && NoteGenerator.Instance.laneNotesLists != null)
        {
            var targetList = NoteGenerator.Instance.laneNotesLists[moveScript.Lane];

            if (targetList.Count > 0 && targetList[0] == moveScript)
            {
                targetList.RemoveAt(0);
            }
        }

        JudgeResult result;

        if (heldRatio >= 0.8f) result = JudgeResult.Perfect;
        else if (heldRatio >= 0.6f) result = JudgeResult.Great;
        else if (heldRatio >= 0.4f) result = JudgeResult.Good;
        else result = JudgeResult.Miss;

        OnNotesJudged(result, isLate: false, timeUntilHit: 0f, moveScript.MyNotesType, destroy: true,false);
    }


    /// <summary>
    /// 押された時間を受け取り、perfectなどの判定を返す関数
    /// </summary>
    /// <param name="pressedTime"></param>
    /// <param name="isLate"></param>
    /// <param name="timeUntilHit"></param>
    /// <returns></returns>
    private JudgeResult? JudgeByTime(double pressedTime, out bool isLate, out float timeUntilHit)
    {
        isLate = false;
        timeUntilHit = 0f;

        // いま判定ラインのスライド範囲内（アクティブレーン）に入っているか
        if (!LaneScript.isActiveLane[moveScript.Lane])
            return null;

        // Unityのフレームのズレを打ち消す正確な曲の時間を逆算
        float exactSongTime = (float)(pressedTime - MusicManagerScript.SongStartRealTime) - InputOffset;

        // 判定ラインからのズレを、OSが検知した入力時間ベースで計算
        timeUntilHit = moveScript.HitTime - exactSongTime;

        float absTimeUntilHit = Mathf.Abs(timeUntilHit);

        // 負の数ならLate（遅い）
        isLate = timeUntilHit < 0;

        if (absTimeUntilHit <= PerfectWindow) return JudgeResult.Perfect;
        else if (absTimeUntilHit <= GreatWindow) return JudgeResult.Great;
        else if (absTimeUntilHit <= GoodWindow) return JudgeResult.Good;
        else if (absTimeUntilHit <= MissWindow) return JudgeResult.Miss;
        else return null;   // どの窓にも入らない＝空振り
    }

    /// <summary>
    /// プレイヤーが誰も叩かないまま、ノーツが後ろへ通り過ぎてしまった時に
    /// 自動でMissにするための専用関数
    /// </summary>
    public void TriggerMissByThrough()
    {
        // check : 管理リストについて
        if (NoteGenerator.Instance != null && NoteGenerator.Instance.laneNotesLists != null)
        {
            var targetList = NoteGenerator.Instance.laneNotesLists[moveScript.Lane];
            if (targetList.Count > 0 && targetList[0] == moveScript) // MoveScriptベースで比較
            {
                targetList.RemoveAt(0);
            }
        }

        OnNotesJudged(JudgeResult.Miss, true, -MissWindow, moveScript.MyNotesType, true, true);
    }


    /// <summary>
    /// ノイズノーツが判定ラインに重なった瞬間に、MoveScriptのUpdateから自動で呼び出される判定関数
    /// </summary>
    public void ExecuteNoiseJudge()
    {
        if (!LaneScript.isActiveLane[moveScript.Lane])
        {
            // リストからこのノーツを削除（TriggerMissByThrough の中身と同じ処理）
            if (NoteGenerator.Instance != null && NoteGenerator.Instance.laneNotesLists != null)
            {
                var targetList = NoteGenerator.Instance.laneNotesLists[moveScript.Lane];
                if (targetList.Count > 0 && targetList[0] == moveScript)
                {
                    targetList.RemoveAt(0);
                }
            }

            // Perfect判定を飛ばす（第2引数はLateかどうか。回避なので適当にfalseでOK）
            // ※ノイズ用のSEを鳴らしたい場合は、OnNotesJudgedのswitch文に後で追加できます
            OnNotesJudged(JudgeResult.Perfect, false, 0f, moveScript.MyNotesType, true, true);
        }

        else
        {
            // 【アクティブ ➔ 接触（Miss!）】
            // 既存の通り過ぎMissの関数をそのまま使い回せば、リスト削除もMiss演出も一発で処理できます！
            TriggerMissByThrough();
        }
    }

    /// <summary>
    /// 判定後の処理を行う関数 (【列挙型】JudgeResult 判定に対する評価, bool 叩くのが遅すぎたか, float 叩くのにズレた時間)
    /// </summary>
    /// <param name="result">判定に対する評価</param>　
    /// <param name="isLate">叩くのが遅すぎたか</param>
    /// <param name="timeUntilHit">叩くのにズレた時間</param>
    /// <param name="destroy">このノーツを破棄するか</param>　
    private void OnNotesJudged(JudgeResult result, bool isLate, float timeUntilHit, NoteType noteType, bool destroy, bool isPlaySE)
    {
        // コンボ更新（Missで切れる、それ以外はつながる）
        if (result == JudgeResult.Miss)
            ResultCounterScript.ResetCombo();

        else
            ResultCounterScript.AddCombo();

        switch (result)
        {
            case JudgeResult.Perfect:
                ResultCounterScript.CountPerfect++;
                if (isPlaySE) SoundEffectScript.Instance.TapNotesSound(noteType);
                JudgeUIScript.Instance.JudgeOutput(moveScript.Lane, JudgeResult.Perfect, isLate);
                break;

            case JudgeResult.Great:
                ResultCounterScript.CountGreat++;
                if (isPlaySE) SoundEffectScript.Instance.TapNotesSound(noteType);
                JudgeUIScript.Instance.JudgeOutput(moveScript.Lane, JudgeResult.Great, isLate);
                break;

            case JudgeResult.Good:
                ResultCounterScript.CountGood++;
                if (isPlaySE) SoundEffectScript.Instance.TapNotesSound(noteType);
                JudgeUIScript.Instance.JudgeOutput(moveScript.Lane, JudgeResult.Good, isLate);
                break;

            case JudgeResult.Miss when !isLate:
                ResultCounterScript.CountMiss++;
                if (isPlaySE) SoundEffectScript.Instance.TapNotesSound(noteType);
                JudgeUIScript.Instance.JudgeOutput(moveScript.Lane, JudgeResult.Miss, isLate);
                break;

            case JudgeResult.Miss:
                ResultCounterScript.CountMiss++;
                JudgeUIScript.Instance.JudgeOutput(moveScript.Lane, JudgeResult.Miss, isLate);
                break;
        }

        if (destroy)
            Destroy(this.gameObject);
    }

    public void TriggerHoldStartMiss()
    {
        var targetList = NoteGenerator.Instance.laneNotesLists[moveScript.Lane];

        if (targetList.Count > 0 && targetList[0] == moveScript)
        {
            targetList.RemoveAt(0);
        }

        OnNotesJudged(JudgeResult.Miss, isLate: true, -MissWindow, moveScript.MyNotesType, destroy: false,true);
    }
}