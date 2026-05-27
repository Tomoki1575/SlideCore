using UnityEngine;
using UnityEngine.InputSystem;

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

    private const float PerfectWindow = 0.05f;
    private const float GreatWindow = 0.10f;
    private const float GoodWindow = 0.25f;
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
    public void ExecuteJudge(double pressedTime)
    {
        // いま判定ラインのスライド範囲内（アクティブレーン）に入っているか
        if (!LaneScript.isActiveLane[moveScript.Lane])
        {
            return;
        }

        // Unityのフレームのズレを打ち消す正確な曲の時間を逆算
        float exactSongTime = (float)(pressedTime - MusicManagerScript.SongStartRealTime);

        exactSongTime -= InputOffset;

        // 判定ラインからのズレを、OSが検知した入力時間ベースで計算
        float timeUntilHit = moveScript.HitTime - exactSongTime;

        float absTimeUntilHit = Mathf.Abs(timeUntilHit);

        // 判定ラインより後ろ（負の数）なら Late（遅い）
        bool isLate = timeUntilHit < 0;

        // 判定枠のチェック（enumを渡すように変更）
        if (absTimeUntilHit <= PerfectWindow) OnNotesJudged(JudgeResult.Perfect, isLate, timeUntilHit);
        else if (absTimeUntilHit <= GreatWindow) OnNotesJudged(JudgeResult.Great, isLate, timeUntilHit);
        else if (absTimeUntilHit <= GoodWindow) OnNotesJudged(JudgeResult.Good, isLate, timeUntilHit);
        else if (absTimeUntilHit <= MissWindow) OnNotesJudged(JudgeResult.Miss, isLate, timeUntilHit);
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

        OnNotesJudged(JudgeResult.Miss, true, -MissWindow);
    }

    /// <summary>
    /// 判定後の処理を行う関数 (【列挙型】JudgeResult 判定に対する評価, bool 叩くのが遅すぎたか, float 叩くのにズレた時間)
    /// </summary>
    /// <param name="result">判定に対する評価</param>　
    /// <param name="isLate">叩くのが遅すぎたか</param>
    /// <param name="timeUntilHit">叩くのにズレた時間</param>
    private void OnNotesJudged(JudgeResult result, bool isLate, float timeUntilHit)
    {
        // todo : ここでスコア加算やエフェクト生成を呼びたい

        switch (result)
        {
            case JudgeResult.Perfect:
                SoundEffectScript.Instance.TapNotesSound();
                break;

            case JudgeResult.Great:
                SoundEffectScript.Instance.TapNotesSound();
                break;

            case JudgeResult.Good:
                SoundEffectScript.Instance.TapNotesSound();
                break;

            case JudgeResult.Miss:
                break;
        }       

        if (isLate)
            Debug.Log($"{result}(Late) (Lane: {moveScript.Lane}, absTimeUntilHit: {Mathf.FloorToInt(timeUntilHit * 1000)}ms)");

        else
            Debug.Log($"{result}(Fast) (Lane: {moveScript.Lane}, absTimeUntilHit: {Mathf.FloorToInt(timeUntilHit * 1000)}ms)");

        Destroy(this.gameObject);
    }
}