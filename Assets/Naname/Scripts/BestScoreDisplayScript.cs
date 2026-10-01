using TMPro;
using UnityEngine;

public class BestScoreDisplayScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI easyScoreText;
    [SerializeField] private TextMeshProUGUI normalScoreText;
    [SerializeField] private TextMeshProUGUI hardScoreText;

    /// <summary>
    /// 選曲中の曲が変わったときに呼ばれる(MusicSelectCoreScript.RefreshPanel から)
    /// </summary>
    public void Refresh(MusicData music)
    {
        ApplyRecord(easyScoreText, music, MusicDataManager.Difficulty.Easy);
        ApplyRecord(normalScoreText, music, MusicDataManager.Difficulty.Normal);
        ApplyRecord(hardScoreText, music, MusicDataManager.Difficulty.Hard);
    }

    private void ApplyRecord(TextMeshProUGUI text, MusicData music, MusicDataManager.Difficulty difficulty)
    {
        if (text == null)
            return;

        ScoreRecord record = (music != null)
            ? ScoreDataManager.FindRecord(music.songID, difficulty)
            : null;

        // 未プレイなら記録が存在しない
        if (record == null)
        {
            text.text = "---";
            return;
        }

        string rank = ResultCounterScript.RankToText(ResultCounterScript.GetRank(record.maxScore));
        string badge = ResultCounterScript.BadgeToShortText(record.bestBadge);

        text.text = $"{record.maxScore:#,0}   {rank}   {badge}".TrimEnd();
    }
}
