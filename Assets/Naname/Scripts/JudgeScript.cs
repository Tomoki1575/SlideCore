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

    public const float PerfectWindow = 0.070f;
    public const float GreatWindow = 0.14f;
    public const float GoodWindow = 0.18f;
    public const float MissWindow = 0.22f;

    public const float NoteLifetimeMargin = 0.08f;      // 入力遅延を吸収するために、ノーツの生存時間を伸ばす

    public const float InputOffset = 0.03f;      // ノーツの「見た目」と「実際の計算による判定」のズレを調整するオフセット。基本的にプレイヤーがいじることは無い

    private JudgeResult holdStartResult = JudgeResult.Miss;

    void Start()
    {
        moveScript = GetComponent<MoveScript>();
    }

    /// <summary>
    /// 押された時間を受け取り、「UI表示+SE再生+ノーツ破棄」を行う。
    /// その後、それが空振りだったか空振りじゃなかったかを返す関数。
    /// （ホールドノーツ以外）
    /// </summary>
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
    public bool ExecuteHoldStartJudge(double pressedTime)
    {
        JudgeResult? result = JudgeByTime(pressedTime, out bool isLate, out float timeUntilHit);

        if (result == null)     // 空振り
            return false;

        OnNotesJudged(result.Value, isLate, timeUntilHit, moveScript.MyNotesType, destroy: false, true);  // UI/SEだけ（破棄はしない）。

        holdStartResult = result.Value;

        moveScript.MarkHoldStarted();   // 始点が押せたことを記録

        return true;
    }

    public void ExecuteHoldEndJudge(float heldRatio, float bandDuration)
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

        // 帯が判定窓より短いと、始点のズレだけで帯を食い潰せてしまう。
        // その場合は押しっぱなしを評価せず、始点の判定をそのまま採用する
        if (bandDuration < GoodWindow * 2f)
        {
            result = holdStartResult;
        }

        else
        {
            if (heldRatio >= 0.85f) result = JudgeResult.Perfect;
            else if (heldRatio >= 0.7f) result = JudgeResult.Great;
            else if (heldRatio >= 0.55f) result = JudgeResult.Good;
            else result = JudgeResult.Miss;
        }

        OnNotesJudged(result, isLate: false, timeUntilHit: 0f, moveScript.MyNotesType, destroy: true, false);
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
        float exactSongTime = (float)(pressedTime - MusicManagerScript.Instance.SongStartRealTime) - InputOffset;

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

            OnNotesJudged(JudgeResult.Perfect, false, 0f, moveScript.MyNotesType, true, true);
        }

        else
        {
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
                if (isPlaySE) SoundEffectScript.Instance.PlayNotesSound(noteType);
                JudgeUIScript.Instance.JudgeOutput(moveScript.Lane, JudgeResult.Perfect, isLate);
                break;

            case JudgeResult.Great:
                ResultCounterScript.CountGreat++;
                if (isPlaySE) SoundEffectScript.Instance.PlayNotesSound(noteType);
                JudgeUIScript.Instance.JudgeOutput(moveScript.Lane, JudgeResult.Great, isLate);
                break;

            case JudgeResult.Good:
                ResultCounterScript.CountGood++;
                if (isPlaySE) SoundEffectScript.Instance.PlayNotesSound(noteType);
                JudgeUIScript.Instance.JudgeOutput(moveScript.Lane, JudgeResult.Good, isLate);
                break;

            case JudgeResult.Miss when !isLate:
                ResultCounterScript.CountMiss++;
                if (isPlaySE) SoundEffectScript.Instance.PlayNotesSound(noteType);
                JudgeUIScript.Instance.JudgeOutput(moveScript.Lane, JudgeResult.Miss, isLate);
                break;

            case JudgeResult.Miss:
                ResultCounterScript.CountMiss++;
                JudgeUIScript.Instance.JudgeOutput(moveScript.Lane, JudgeResult.Miss, isLate);
                break;
        }

        if (destroy)
            Destroy(gameObject);
    }

    /// <summary>
    /// 叩かれないまま判定を過ぎたノーツをMissにして、その場で破棄する。<br/>
    /// レーンのリストからも取り除くので、次のノーツが叩けるようになる。
    /// </summary>
    public void TriggerMissByThrough()
    {
        if (NoteGenerator.Instance == null || NoteGenerator.Instance.laneNotesLists == null)
            return;

        var targetList = NoteGenerator.Instance.laneNotesLists[moveScript.Lane];
        if (targetList.Count > 0 && targetList[0] == moveScript) // MoveScriptベースで比較        
            targetList.RemoveAt(0);

        OnNotesJudged(JudgeResult.Miss, isLate: true, -MissWindow, moveScript.MyNotesType, destroy: true, true);
    }

    /// <summary>
    /// Hold の始点が叩かれないまま過ぎた時に、始点だけをMissにする。帯の判定がまだ残っているので、ノーツは破棄しない。<br/>
    /// レーンのリストからは取り除くので、次のノーツが叩けるようになる。
    /// </summary>
    public void TriggerHoldStartMiss()
    {
        if (NoteGenerator.Instance == null || NoteGenerator.Instance.laneNotesLists == null)
            return;

        var targetList = NoteGenerator.Instance.laneNotesLists[moveScript.Lane];

        if (targetList.Count > 0 && targetList[0] == moveScript)
            targetList.RemoveAt(0);

        // 始点でMissだった時に、帯まで一緒に消さないようにする
        OnNotesJudged(JudgeResult.Miss, isLate: true, -MissWindow, moveScript.MyNotesType, destroy: false, true);
    }
}
