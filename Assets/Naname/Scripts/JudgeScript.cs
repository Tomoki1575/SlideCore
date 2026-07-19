using TMPro;
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

    private const float PerfectWindow = 0.06f;
    private const float GreatWindow = 0.12f;
    private const float GoodWindow = 0.24f;
    private const float MissWindow = 0.4f;

    public static float InputOffset { get; private set; } = 0.1f;

    void Start()
    {
        moveScript = GetComponent<MoveScript>();
    }

    /// <summary>
    /// InputManagerScriptでキー入力が検知された瞬間に、ピンポイントで呼び出される判定関数 (double 押された時間)
    /// </summary>
    /// <param name="pressedTime">新Input Systemが記録した、物理的にキーが押された正確な時間(context.time)</param>
    public bool ExecuteJudge(double pressedTime)
    {
        // いま判定ラインのスライド範囲内（アクティブレーン）に入っているか
        if (!LaneScript.isActiveLane[moveScript.Lane])
        {
            return false;
        }

        // Unityのフレームのズレを打ち消す正確な曲の時間を逆算
        float exactSongTime = (float)(pressedTime - MusicManagerScript.SongStartRealTime);

        exactSongTime -= InputOffset;

        // 判定ラインからのズレを、OSが検知した入力時間ベースで計算
        float timeUntilHit = moveScript.HitTime - exactSongTime;

        float absTimeUntilHit = Mathf.Abs(timeUntilHit);

        // 負の数ならLate（遅い）
        bool isLate = timeUntilHit < 0;

        if (absTimeUntilHit <= PerfectWindow)
        {
            OnNotesJudged(JudgeResult.Perfect, isLate, timeUntilHit, moveScript.MyNotesType);
            return true;
        }

        else if (absTimeUntilHit <= GreatWindow)
        {
            OnNotesJudged(JudgeResult.Great, isLate, timeUntilHit, moveScript.MyNotesType);
            return true;
        }

        else if (absTimeUntilHit <= GoodWindow)
        {
            OnNotesJudged(JudgeResult.Good, isLate, timeUntilHit, moveScript.MyNotesType);
            return true;
        }

        else if (absTimeUntilHit <= MissWindow)
        {
            OnNotesJudged(JudgeResult.Miss, isLate, timeUntilHit, moveScript.MyNotesType);
            return true;
        }

        else
        {
            return false;
        }
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

        OnNotesJudged(JudgeResult.Miss, true, -MissWindow, moveScript.MyNotesType);
    }

    /// <summary>
    /// 判定後の処理を行う関数 (【列挙型】JudgeResult 判定に対する評価, bool 叩くのが遅すぎたか, float 叩くのにズレた時間)
    /// </summary>
    /// <param name="result">判定に対する評価</param>　
    /// <param name="isLate">叩くのが遅すぎたか</param>
    /// <param name="timeUntilHit">叩くのにズレた時間</param>
    private void OnNotesJudged(JudgeResult result, bool isLate, float timeUntilHit,NoteType noteType)
    {
        // todo : ここでスコア加算やエフェクト生成を呼びたい

        switch (result)
        {
            case JudgeResult.Perfect:
                SoundEffectScript.Instance.TapNotesSound(noteType);
                JudgeUIScript.Instance.JudgeOutput(moveScript.Lane,JudgeResult.Perfect,isLate);
                break;

            case JudgeResult.Great:
                SoundEffectScript.Instance.TapNotesSound(noteType);
                JudgeUIScript.Instance.JudgeOutput(moveScript.Lane, JudgeResult.Great, isLate);
                break;

            case JudgeResult.Good:
                SoundEffectScript.Instance.TapNotesSound(noteType);
                JudgeUIScript.Instance.JudgeOutput(moveScript.Lane, JudgeResult.Good, isLate);
                break;

            case JudgeResult.Miss when !isLate:
                SoundEffectScript.Instance.TapNotesSound(noteType);
                JudgeUIScript.Instance.JudgeOutput(moveScript.Lane, JudgeResult.Miss, isLate);
                break;

            case JudgeResult.Miss:
                JudgeUIScript.Instance.JudgeOutput(moveScript.Lane, JudgeResult.Miss, isLate);
                break;
        }

        if (isLate)
            Debug.Log($"{result}(Late) (Lane: {moveScript.Lane}, absTimeUntilHit: {Mathf.FloorToInt(timeUntilHit * 1000)}ms)");

        else
            Debug.Log($"{result}(Fast) (Lane: {moveScript.Lane}, absTimeUntilHit: {Mathf.FloorToInt(timeUntilHit * 1000)}ms)");

        Destroy(this.gameObject);
    }

    /// <summary>
    /// ノイズノーツが判定ラインに重なった瞬間に、MoveScriptのUpdateから自動で呼び出される判定関数
    /// </summary>
    public void ExecuteNoiseJudge()
    {
        if(!LaneScript.isActiveLane[moveScript.Lane])
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
            OnNotesJudged(JudgeResult.Perfect, false, 0f, moveScript.MyNotesType);
        }

        else
        {
            // 【アクティブ ➔ 接触（Miss!）】
            // 既存の通り過ぎMissの関数をそのまま使い回せば、リスト削除もMiss演出も一発で処理できます！
            TriggerMissByThrough();
        }
    }
}