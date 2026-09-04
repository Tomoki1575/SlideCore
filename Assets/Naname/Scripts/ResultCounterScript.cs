using UnityEngine;

public enum ClearBadge
{
    AllPerfect,
    FullComboPlus,
    FullCombo,
    Finish
}

public enum ScoreRank
{
    SSS_Plus,
    SSS,
    SS,
    S,
    A,
    B,
    C,
    D
}

public class ResultCounterScript : MonoBehaviour
{
    public static int CountPerfect = 0;
    public static int CountGreat = 0;
    public static int CountGood = 0;
    public static int CountMiss = 0;

    public static int CountCombo = 0;
    public static int MaxCombo = 0;

    public static int TotalJudgments = 0;   // 全ノーツの判定回数（スコアの分母）

    public static void ResetCounts()
    {
        CountPerfect = 0;
        CountGreat = 0;
        CountGood = 0;
        CountMiss = 0;

        CountCombo = 0;
        MaxCombo = 0;

        TotalJudgments = 0;
    }

    // ヒット時にコンボを増やし、最高記録を更新
    public static void AddCombo()
    {
        CountCombo++;
        if (CountCombo > MaxCombo)
            MaxCombo = CountCombo;
    }

    // Miss時：コンボを切る
    public static void ResetCombo()
    {
        CountCombo = 0;
    }

    public static int GetScore()
    {
        if (TotalJudgments <= 0) return 0;

        int MaxScore = TotalJudgments;

        // 判定ごとの重み(Miss は 0 なので足さない)
        float weighted = CountPerfect + CountGreat * 0.8f + CountGood * 0.5f;

        return Mathf.RoundToInt(weighted / MaxScore * 1_000_000f);
    }

    public static ClearBadge GetBadge()
    {
        // 上から順にチェック（厳しい→ゆるい）
        if (CountMiss == 0 && CountGood == 0 && CountGreat == 0)
            return ClearBadge.AllPerfect;

        if (CountMiss == 0 && CountGood == 0)
            return ClearBadge.FullComboPlus;

        if (CountMiss == 0)
            return ClearBadge.FullCombo;

        return ClearBadge.Finish;
    }

    public static ScoreRank GetRank()
    {
        int score = GetScore();

        if (score >= 1_000_000) return ScoreRank.SSS_Plus;
        if (score >= 990_000) return ScoreRank.SSS;
        if (score >= 980_000) return ScoreRank.SS;
        if (score >= 950_000) return ScoreRank.S;
        if (score >= 900_000) return ScoreRank.A;
        if (score >= 800_000) return ScoreRank.B;
        if (score >= 650_000) return ScoreRank.C;
        return ScoreRank.D;
    }
}