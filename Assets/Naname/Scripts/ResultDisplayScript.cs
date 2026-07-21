using UnityEngine;
using TMPro;

public class ResultDisplayScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI badgeText;
    [SerializeField] private TextMeshProUGUI perfectText;
    [SerializeField] private TextMeshProUGUI greatText;
    [SerializeField] private TextMeshProUGUI goodText;
    [SerializeField] private TextMeshProUGUI missText;
    [SerializeField] private TextMeshProUGUI maxComboText;

    private void Start()
    {
        scoreText.text = ResultCounterScript.GetScore().ToString();
        rankText.text = ResultCounterScript.GetRank().ToString();
        badgeText.text = BadgeToText(ResultCounterScript.GetBadge());

        perfectText.text = ResultCounterScript.CountPerfect.ToString();
        greatText.text = ResultCounterScript.CountGreat.ToString();
        goodText.text = ResultCounterScript.CountGood.ToString();
        missText.text = ResultCounterScript.CountMiss.ToString();

        maxComboText.text = ResultCounterScript.MaxCombo.ToString();
    }

    // enum は "+" を含められないので、バッジだけ表示用に変換
    private string BadgeToText(ClearBadge badge)
    {
        switch (badge)
        {
            case ClearBadge.AllPerfect: return "ALL PERFECT";
            case ClearBadge.FullComboPlus: return "FULL COMBO+";
            case ClearBadge.FullCombo: return "FULL COMBO";
            default: return "CLEAR";
        }
    }
}